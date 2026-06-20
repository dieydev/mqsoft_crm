using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_CRM.Infrastructure.Data;
using AI_CRM.WebMvc.Models;

namespace AI_CRM.WebMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
                if (currentRole != "User")
                {
                    return RedirectToAction("Index", "Admin");
                }
                
                // Thu hồi cookie bị kẹt của bản cũ
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Phiên đăng nhập cũ đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Auth");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Simple login: check username and verify bcrypt password
                var user = await _context.NguoiDungs
                    .Include(u => u.VaiTro)
                    .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

                if (user != null)
                {
                    bool isValidPassword = false;
                    try
                    {
                        // Verify BCrypt hash
                        isValidPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                    }
                    catch (Exception)
                    {
                        // Handle cases where the existing password in DB is not a valid bcrypt hash
                        // Fallback for development if plain text was used before adding bcrypt
                        isValidPassword = (user.PasswordHash == model.Password);
                    }

                    if (isValidPassword)
                    {
                        var roleName = user.VaiTro?.RoleName ?? "User";
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Role, roleName)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // Phân quyền chuyển trang
                        if (roleName != "User")
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            // Chặn người dùng lạ vào Admin dù có returnUrl
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                // If login fails, check if it's admin/admin as a fallback (for development)
                if (model.Username == "admin" && model.Password == "admin")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "admin"),
                        new Claim(ClaimTypes.NameIdentifier, "0"),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Admin");
                }
                
                // Fallback for user/user
                if (model.Username == "user" && model.Password == "user")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "user"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Vô hiệu hóa tính năng tự do đăng ký cho hệ thống nội bộ
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            // Vô hiệu hóa tính năng tự do đăng ký cho hệ thống nội bộ
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult GoogleLogin(string returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleResponse(string returnUrl = null)
        {
            var googleAuth = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (googleAuth.Succeeded)
            {
                var emailClaim = googleAuth.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var nameClaim = googleAuth.Principal.FindFirst(ClaimTypes.Name)?.Value;
                
                string roleName = "User"; // Mặc định khách vãng lai

                if (!string.IsNullOrEmpty(emailClaim))
                {
                    // Tìm xem nhân viên này có trong CSDL không
                    var nhanVien = await _context.NhanVienPhuTrachs
                        .Include(nv => nv.NguoiDung)
                        .ThenInclude(nd => nd.VaiTro)
                        .FirstOrDefaultAsync(nv => nv.Email == emailClaim && nv.IsActive);

                    if (nhanVien != null && nhanVien.NguoiDung != null)
                    {
                        roleName = nhanVien.NguoiDung.VaiTro?.RoleName ?? "Employee";
                    }
                }

                var claims = googleAuth.Principal.Claims.ToList();
                claims.Add(new Claim(ClaimTypes.Role, roleName));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                // Xóa ExternalCookie
                await HttpContext.SignOutAsync("ExternalCookie");

                if (roleName != "User")
                {
                    // Chỉ cấp Cookie chính thức cho nhân viên hợp lệ
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    // Chặn người dùng lạ, KHÔNG cho đăng nhập
                    TempData["ErrorMessage"] = "Truy cập bị từ chối. Tài khoản Gmail này không có quyền truy cập hệ thống nội bộ.";
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
