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
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
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

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        // Phân quyền chuyển trang
                        if (roleName != "User")
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            // Tài khoản thường thì ra trang chủ
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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Username hoặc Email đã tồn tại chưa
                var userExists = await _context.NguoiDungs.AnyAsync(u => u.Username == model.Username);
                var emailExists = await _context.NhanVienPhuTrachs.AnyAsync(nv => nv.Email == model.Email);
                
                if (userExists)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                if (model.AgreeTerms)
                {
                    // Lấy Role mặc định cho nhân viên hoặc User
                    var defaultRole = await _context.VaiTros.FirstOrDefaultAsync(r => r.RoleName == "User") 
                        ?? await _context.VaiTros.FirstOrDefaultAsync();

                    // Băm mật khẩu bằng BCrypt
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    var newUser = new AI_CRM.Domain.Entities.NguoiDung 
                    { 
                        Username = model.Username, 
                        PasswordHash = hashedPassword,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        RoleId = defaultRole?.RoleId ?? 0
                    };
                    
                    _context.NguoiDungs.Add(newUser);
                    await _context.SaveChangesAsync();
                    
                    // Thêm thông tin vào bảng NhanVienPhuTrach để lưu Email
                    var newStaff = new AI_CRM.Domain.Entities.NhanVienPhuTrach
                    {
                        UserId = newUser.UserId,
                        Email = model.Email,
                        FullName = model.Username, // Lấy tạm username làm tên
                        IsActive = true
                    };
                    
                    _context.NhanVienPhuTrachs.Add(newStaff);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("Login", new { returnUrl = "/Admin/Index" });
                }
            }
            return View(model);
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
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (result.Succeeded)
            {
                var currentRole = result.Principal.FindFirst(ClaimTypes.Role)?.Value ?? "User";
                if (currentRole != "User")
                {
                    return LocalRedirect(returnUrl ?? "/Admin/Index");
                }
                return LocalRedirect(returnUrl ?? "/Home/Index");
            }

            var googleAuth = await HttpContext.AuthenticateAsync("Google");
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
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                if (roleName != "User")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
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
