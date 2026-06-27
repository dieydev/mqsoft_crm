using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using AI_CRM.WebMvc.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AI_CRM.WebMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                var (isValid, roleName, userId) = await _authService.ValidateUserAsync(model.Username, model.Password);

                if (isValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
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
                var roleName = "User"; // Mặc định khách vãng lai

                if (!string.IsNullOrEmpty(emailClaim))
                {
                    roleName = await _authService.GetUserRoleByEmailAsync(emailClaim);
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
