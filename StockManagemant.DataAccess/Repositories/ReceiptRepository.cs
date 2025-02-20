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

        // ✅ **Filtreleme ile Toplam Fiş Sayısını Getir**
        public async Task<int> GetTotalCountAsync(ReceiptFilter filter)
        {
            return await _context.Receipts
                .Where(filter.GetFilterExpression())
                .CountAsync();
        }

        // ✅ **Sayfalama ile fişleri getir**
        public async Task<IEnumerable<Receipt>> GetPagedReceiptsAsync(ReceiptFilter filter, int page, int pageSize)
        {
            return await _context.Receipts
                .Where(filter.GetFilterExpression())
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ✅ **Yeni fiş ekleme (Toplam tutar başlangıçta 0 olmalı)**
        public async Task<int> AddReceiptAsync(Receipt receipt)
        {
            receipt.TotalAmount = 0; // **Yeni fiş 0 TL ile başlar**
            await _context.Receipts.AddAsync(receipt);
            await _context.SaveChangesAsync();
            return receipt.Id; // **Yeni fiş ID'sini geri döndür**
        }

        // ✅ **Fiş güncelleme (TotalAmount yeniden hesaplanmalı)**
        public async Task UpdateReceiptAsync(Receipt receipt)
        {
            // **Fişe bağlı detaylardan toplam tutarı hesapla**
            receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receipt.Id);
            _context.Receipts.Update(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(Receipt receipt)
        {
            if (receipt.Id > 0)
            {
                // **Fişe bağlı detaylardan toplam tutarı hesapla**
                receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receipt.Id);
                _context.Receipts.Update(receipt);
            }
            else
            {
                await _context.Receipts.AddAsync(receipt);
            }
            await _context.SaveChangesAsync();
        }

        // ✅ **Fiş silme işlemi (Soft Delete)**
        public async Task DeleteReceiptAsync(int id)
        {
            var receipt = await GetByIdAsync(id);
            if (receipt != null)
            {
                // **Önce fişe bağlı tüm detayları Soft Delete ile işaretle**
                await _receiptDetailRepository.DeleteByReceiptIdAsync(id);

                // **Fişi de Soft Delete olarak işaretle**
                receipt.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }


}
