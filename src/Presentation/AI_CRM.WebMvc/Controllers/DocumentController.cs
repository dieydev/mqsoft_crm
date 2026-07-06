using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IEmployeeService _employeeService;

        // Whitelist file extensions được phép upload
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg", ".zip", ".rar" };
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        public DocumentController(IDocumentService documentService, IEmployeeService employeeService)
        {
            _documentService = documentService;
            _employeeService = employeeService;
        }

        /// <summary>
        /// Lấy EmployeeId từ Claims của user đang đăng nhập
        /// </summary>
        private async Task<int> GetCurrentEmployeeIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employees = await _employeeService.GetEmployeesAsync(null);
                var employee = employees.FirstOrDefault(e => e.UserId == userId);
                if (employee != null) return employee.EmployeeId;
            }
            return 1; // Fallback nếu không tìm thấy
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var docs = await _documentService.GetDocumentsAsync(searchString, categoryId);
            
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName");
            ViewBag.SearchString = searchString;
            ViewBag.CurrentCategory = categoryId;

            return View(docs);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AI_CRM.Domain.Entities.TaiLieuNoiBo document, string TagsInput, Microsoft.AspNetCore.Http.IFormFile uploadFile)
        {
            ModelState.Remove("NhomTaiLieu");
            ModelState.Remove("NhanVienPhuTrach");
            ModelState.Remove("TaiLieuTags");
            ModelState.Remove("FilePath"); // Will set manually

            if (ModelState.IsValid)
            {
                if (uploadFile != null && uploadFile.Length > 0)
                {
                    // Validate file extension (chống upload file nguy hiểm)
                    var ext = System.IO.Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("FilePath", $"Định dạng file '{ext}' không được phép. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}");
                        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
                        return View(document);
                    }

                    // Validate file size
                    if (uploadFile.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("FilePath", "Dung lượng file không được vượt quá 50MB.");
                        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
                        return View(document);
                    }

                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = System.Guid.NewGuid().ToString() + ext;
                    var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }

                    document.FilePath = "/uploads/documents/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("FilePath", "Vui lòng chọn file tài liệu.");
                    ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
                    return View(document);
                }

                // Lấy EmployeeId thực tế từ user đang đăng nhập
                document.UploadedBy = await GetCurrentEmployeeIdAsync();

                var tagsList = TagsInput?.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                await _documentService.CreateDocumentAsync(document, tagsList);
                TempData["SuccessMessage"] = "Upload tài liệu thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
            return View(document);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doc = await _documentService.GetDocumentByIdAsync(id.Value);
            if (doc == null || doc.IsDeleted) return NotFound();

            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", doc.CategoryId);
            ViewBag.TagsInput = string.Join(", ", doc.TaiLieuTags.Select(t => t.TagName));

            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AI_CRM.Domain.Entities.TaiLieuNoiBo document, string TagsInput, Microsoft.AspNetCore.Http.IFormFile uploadFile)
        {
            if (id != document.DocumentId) return NotFound();

            ModelState.Remove("NhomTaiLieu");
            ModelState.Remove("NhanVienPhuTrach");
            ModelState.Remove("TaiLieuTags");
            ModelState.Remove("FilePath");

            if (ModelState.IsValid)
            {
                if (uploadFile != null && uploadFile.Length > 0)
                {
                    // Validate file extension
                    var ext = System.IO.Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("FilePath", $"Định dạng file '{ext}' không được phép.");
                        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
                        return View(document);
                    }

                    // Validate file size
                    if (uploadFile.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("FilePath", "Dung lượng file không được vượt quá 50MB.");
                        ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
                        return View(document);
                    }

                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = System.Guid.NewGuid().ToString() + ext;
                    var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }

                    document.FilePath = "/uploads/documents/" + uniqueFileName;
                }

                var tagsList = TagsInput?.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                var success = await _documentService.UpdateDocumentAsync(document, tagsList);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật tài liệu thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _documentService.GetCategoriesAsync(), "CategoryId", "CategoryName", document.CategoryId);
            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _documentService.DeleteDocumentAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa tài liệu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null || doc.IsDeleted || string.IsNullOrEmpty(doc.FilePath)) return NotFound();

            // Chống path traversal: validate đường dẫn không chứa ".."
            if (doc.FilePath.Contains("..") || doc.FilePath.Contains("~"))
                return BadRequest("Đường dẫn file không hợp lệ.");

            var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
            
            // Đảm bảo file nằm trong thư mục uploads
            var fullPath = System.IO.Path.GetFullPath(filePath);
            var uploadsRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads"));
            if (!fullPath.StartsWith(uploadsRoot))
                return BadRequest("Truy cập file không hợp lệ.");

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var memory = new System.IO.MemoryStream();
            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = "application/octet-stream";
            var fileName = System.IO.Path.GetFileName(fullPath);

            return File(memory, contentType, fileName);
        }
    }
}
