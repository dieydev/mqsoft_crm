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

        public async Task<List<TaiLieuNoiBo>> GetDocumentsAsync()
        {
            return await _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Include(d => d.NhanVienPhuTrach)
                .ToListAsync();
        }
    }
}
