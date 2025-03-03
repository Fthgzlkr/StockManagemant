using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class ReceiptDetailRepository : Repository<ReceiptDetail>
    {
        public ReceiptDetailRepository(AppDbContext context) : base(context) { }

        // ✅ **Belirli bir fişe ait aktif fiş detaylarını getir**
        public async Task<List<ReceiptDetail>> GetByReceiptIdAsync(int receiptId)
        {
            return await _dbSet
                .Where(rd => rd.ReceiptId == receiptId && !rd.IsDeleted)
                .ToListAsync();
        }

        // ✅ **Fişe bağlı detaylardan toplam tutarı hesapla ve fişi güncelle**
        public async Task UpdateReceiptTotal(int receiptId)
        {
            var total = await GetTotalAmountByReceiptIdAsync(receiptId);
            var receipt = await _context.Receipts.FindAsync(receiptId);
            if (receipt != null)
            {
                receipt.TotalAmount = total;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ **Fişin toplam tutarını getir (Soft delete yapılmışları hariç tutar)**
        public async Task<decimal> GetTotalAmountByReceiptIdAsync(int receiptId)
        {
            return await _dbSet
                .Where(rd => rd.ReceiptId == receiptId && !rd.IsDeleted)
                .SumAsync(rd => (decimal?)rd.SubTotal) ?? 0;
        }
    }
}
