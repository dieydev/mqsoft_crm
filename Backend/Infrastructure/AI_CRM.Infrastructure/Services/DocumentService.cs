using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaiLieuNoiBo>> GetDocumentsAsync(string searchString = null, int? categoryId = null)
        {
            var query = _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Include(d => d.NhanVienPhuTrach)
                .Include(d => d.TaiLieuTags)
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(d => d.Title.ToLower().Contains(lowerSearch) || 
                                         d.Description.ToLower().Contains(lowerSearch));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
        }

        public async Task<TaiLieuNoiBo> GetDocumentByIdAsync(int id)
        {
            return await _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Include(d => d.TaiLieuTags)
                .FirstOrDefaultAsync(d => d.DocumentId == id && !d.IsDeleted);
        }

        public async Task<bool> CreateDocumentAsync(TaiLieuNoiBo document, IEnumerable<string> tags)
        {
            document.UploadDate = System.DateTime.Now;
            _context.TaiLieuNoiBos.Add(document);
            await _context.SaveChangesAsync();

            if (tags != null)
            {
                foreach (var tagName in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        _context.TaiLieuTags.Add(new TaiLieuTag
                        {
                            DocumentId = document.DocumentId,
                            TagName = tagName.Trim()
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateDocumentAsync(TaiLieuNoiBo document, IEnumerable<string> tags)
        {
            var existing = await _context.TaiLieuNoiBos
                .Include(d => d.TaiLieuTags)
                .FirstOrDefaultAsync(d => d.DocumentId == document.DocumentId);

            if (existing == null) return false;

            existing.Title = document.Title;
            existing.Description = document.Description;
            existing.CategoryId = document.CategoryId;

            if (!string.IsNullOrEmpty(document.FilePath))
            {
                existing.FilePath = document.FilePath;
            }

            // Update tags
            _context.TaiLieuTags.RemoveRange(existing.TaiLieuTags);
            
            if (tags != null)
            {
                foreach (var tagName in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        _context.TaiLieuTags.Add(new TaiLieuTag
                        {
                            DocumentId = document.DocumentId,
                            TagName = tagName.Trim()
                        });
                    }
                }
            }

            _context.TaiLieuNoiBos.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.TaiLieuNoiBos.FindAsync(id);
            if (document != null)
            {
                document.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<NhomTaiLieu>> GetCategoriesAsync()
        {
            return await _context.NhomTaiLieus.ToListAsync();
        }

        public async Task<List<TaiLieuTag>> GetDocumentTagsAsync(int documentId)
        {
            return await _context.TaiLieuTags.Where(t => t.DocumentId == documentId).ToListAsync();
        }
    }
}
