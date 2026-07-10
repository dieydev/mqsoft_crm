using Microsoft.AspNetCore.Mvc;
using AI_CRM.Domain.Entities;
using AI_CRM.WebMvc.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /User - Danh sách người dùng với tìm kiếm & lọc
        public async Task<IActionResult> Index(string searchString, int? roleFilter, int? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRole"] = roleFilter;
            ViewData["CurrentStatus"] = statusFilter;

            var users = await _userService.GetUsersAsync(searchString, roleFilter, statusFilter);
            ViewBag.Roles = await _userService.GetRolesAsync();

            return View(users);
        }

        // GET: /User/Create - Form tạo tài khoản mới
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _userService.GetRolesAsync();
            return View();
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new NguoiDung
                {
                    Username = model.Username,
                    RoleId = model.RoleId,
                    IsActive = model.IsActive
                };

                var result = await _userService.CreateUserAsync(user, model.Password);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Tạo tài khoản \"{model.Username}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Username", result.ErrorMessage);
                }
            }

            ViewBag.Roles = await _userService.GetRolesAsync();
            return View(model);
        }

        // GET: /User/Edit/5 - Form sửa tài khoản
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            };

            ViewBag.Roles = await _userService.GetRolesAsync();
            return View(model);
        }

        // POST: /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserAsync(id, model.Username, model.RoleId, model.IsActive);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Cập nhật tài khoản \"{model.Username}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Username", result.ErrorMessage);
                }
            }

            ViewBag.Roles = await _userService.GetRolesAsync();
            return View(model);
        }

        // GET: /User/Details/5 - Xem chi tiết tài khoản
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUserDetailsAsync(id.Value);

            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /User/ToggleStatus/5 - Khóa/Mở khóa tài khoản
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.StatusMessage;
            }
            else
            {
                TempData["ErrorMessage"] = result.StatusMessage;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /User/Delete/5 - Xóa tài khoản (chỉ Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _userService.DeleteUserAsync(id, currentUserId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Đã xóa tài khoản thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /User/ChangeRole - Thay đổi vai trò (phân quyền)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            var result = await _userService.ChangeUserRoleAsync(model.UserId, model.NewRoleId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Đã phân quyền \"{result.NewRole?.RoleName}\" thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /User/ResetPassword - Admin reset mật khẩu cho user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _userService.ResetUserPasswordAsync(model.UserId, model.NewPassword);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Đã đặt lại mật khẩu cho tài khoản \"{model.Username}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Profile - Hồ sơ cá nhân
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var nhanVien = await _userService.GetUserProfileByUserIdAsync(userId);
                return View(nhanVien);
            }
            else
            {
                // Fallback for Google login without linked user ID
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return RedirectToAction("Index", "Admin");

                var nhanVien = await _userService.GetUserProfileByEmailAsync(email);
                return View(nhanVien);
            }
        }

        // POST: /User/UpdateAvatar - Cập nhật ảnh đại diện
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(Microsoft.AspNetCore.Http.IFormFile avatar)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.";
                return RedirectToAction("Profile");
            }

            if (avatar != null && avatar.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = System.IO.Path.GetExtension(avatar.FileName).ToLowerInvariant();
                
                if (!System.Linq.Enumerable.Contains(allowedExtensions, extension))
                {
                    TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh định dạng .jpg, .jpeg, .png, .gif";
                    return RedirectToAction("Profile");
                }
                
                if (avatar.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 5MB";
                    return RedirectToAction("Profile");
                }

                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!System.IO.Directory.Exists(uploadsFolder))
                {
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                }

                // Luôn lưu thành avatar_{userId}.jpg để không phải lưu thêm thông tin vào database
                var fileName = $"avatar_{userId}.jpg";
                var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }
                
                TempData["AvatarVersion"] = DateTime.Now.Ticks.ToString();
                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
            }
            
            return RedirectToAction("Profile");
        }

        // GET: /User/ChangePassword - Form đổi mật khẩu
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    TempData["ErrorMessage"] = "Không xác định được tài khoản đang đăng nhập.";
                    return View(model);
                }

                var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    if (result.ErrorMessage == "Mật khẩu hiện tại không đúng.")
                    {
                        ModelState.AddModelError("CurrentPassword", result.ErrorMessage);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = result.ErrorMessage;
                    }
                }
            }

            return View(model);
        }

        // GET: /User/Settings - Cài đặt hệ thống
        public IActionResult Settings()
        {
            return View();
        }
    }
}
