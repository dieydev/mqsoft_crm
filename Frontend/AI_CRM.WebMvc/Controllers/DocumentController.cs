using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public async Task<IActionResult> Index()
        {
            var docs = await _documentService.GetDocumentsAsync();
            return View(docs);
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
