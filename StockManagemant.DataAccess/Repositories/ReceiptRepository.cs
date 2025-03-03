using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class ReceiptRepository : Repository<Receipt>
    {
        private readonly ReceiptDetailRepository _receiptDetailRepository;

        public ReceiptRepository(AppDbContext context, ReceiptDetailRepository receiptDetailRepository)
            : base(context)
        {
            _receiptDetailRepository = receiptDetailRepository;
        }

        // Toplam Fiş sayısı 
        public async Task<int> GetTotalCountAsync(ReceiptFilter filter)
        {
            return await _context.Receipts
                .Where(filter.GetFilterExpression())
                .CountAsync();
        }

        // Sayfalama ile fişler
        public async Task<IEnumerable<Receipt>> GetPagedReceiptsAsync(ReceiptFilter filter, int page, int pageSize)
        {
            return await _context.Receipts
                .Where(filter.GetFilterExpression())
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Yeni Fiş ekleme
        public async Task<int> AddReceiptAsync(Receipt receipt)
        {
            try
            {
                if (receipt.WarehouseId == 0)
                {
                    throw new Exception("Hata: Depo ID boş olamaz!");
                }

                receipt.TotalAmount = 0;
                await _context.Receipts.AddAsync(receipt);
                await _context.SaveChangesAsync();
                return receipt.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddReceiptAsync] Hata: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }




        // Fiş Güncelleme
        public async Task UpdateReceiptAsync(Receipt receipt)
        {
           
            receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receipt.Id);
            _context.Receipts.Update(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(Receipt receipt)
        {
            if (receipt.Id > 0)
            {
              
                receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receipt.Id);
                _context.Receipts.Update(receipt);
            }
            else
            {
                await _context.Receipts.AddAsync(receipt);
            }
            await _context.SaveChangesAsync();
        }

        //Fiş silme işlemi (Soft Delete)
        public async Task DeleteReceiptAsync(int id)
        {
            var receipt = await GetByIdAsync(id);
            if (receipt != null)
            {
                // Önce fişe bağlı tüm detayları 
                var details = await _receiptDetailRepository.GetByReceiptIdAsync(id);

                foreach (var detail in details)
                {
                    await _receiptDetailRepository.DeleteAsync(detail.Id); 
                }

               
                receipt.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

    }


}
