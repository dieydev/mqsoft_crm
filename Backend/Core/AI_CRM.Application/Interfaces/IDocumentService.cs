using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<List<TaiLieuNoiBo>> GetDocumentsAsync(string searchString = null, int? categoryId = null);
        Task<TaiLieuNoiBo> GetDocumentByIdAsync(int id);
        Task<bool> CreateDocumentAsync(TaiLieuNoiBo document, IEnumerable<string> tags);
        Task<bool> UpdateDocumentAsync(TaiLieuNoiBo document, IEnumerable<string> tags);
        Task<bool> DeleteDocumentAsync(int id);
        
        Task<List<NhomTaiLieu>> GetCategoriesAsync();
        Task<List<TaiLieuTag>> GetDocumentTagsAsync(int documentId);
    }
}
