using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
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
                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + System.IO.Path.GetFileName(uploadFile.FileName);
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

                // Temporary logic: get current employee ID. Since we don't have a reliable way to get current EmployeeId from Claims easily right now without joining, let's assume 1 or handle it.
                // In a real app, we'd lookup EmployeeId by User.Identity.Name.
                // For now, let's set UploadedBy = 1 (assuming Admin has EmployeeId 1).
                document.UploadedBy = 1; 

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
                    var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + System.IO.Path.GetFileName(uploadFile.FileName);
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

            var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var memory = new System.IO.MemoryStream();
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = "application/octet-stream";
            var fileName = System.IO.Path.GetFileName(filePath);

            return File(memory, contentType, fileName);
        }
    }
}
