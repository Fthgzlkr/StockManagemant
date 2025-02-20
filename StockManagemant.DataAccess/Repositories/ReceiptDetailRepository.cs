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

        private readonly ProductRepository _productRepository;
        public ReceiptDetailRepository(AppDbContext context,ProductRepository productRepository) 
            : base(context) {
            _productRepository = productRepository;
        }


        // ✅ **Belirli bir fişe ait aktif fiş detaylarını getir**
        public async Task<List<ReceiptDetail>> GetByReceiptIdAsync(int receiptId)
        {
            return await _dbSet
                .Where(rd => rd.ReceiptId == receiptId && !rd.IsDeleted)
                .ToListAsync();
        }

        // ✅ **Yeni bir ürün fişe eklenirken fiyatı sabitlenmeli**
        public override async Task AddAsync(ReceiptDetail receiptDetail)
        {
            var product = await _context.Products.FindAsync(receiptDetail.ProductId);
            if (product != null)
            {
                receiptDetail.ProductPriceAtSale = product.Price ?? 0; // Eğer `Price` null ise, 0 ata
                receiptDetail.SubTotal = receiptDetail.Quantity * receiptDetail.ProductPriceAtSale;
            }
            await base.AddAsync(receiptDetail);
            await UpdateReceiptTotal(receiptDetail.ReceiptId);
        }

        // ✅ **Fiş detayını güncelle (adet değişirse subtotal hesaplanmalı)**
        public override async Task UpdateAsync(ReceiptDetail receiptDetail)
        {
            var product = await _productRepository.GetByIdAsync(receiptDetail.ProductId);

            receiptDetail.SubTotal = receiptDetail.Quantity * receiptDetail.ProductPriceAtSale;
            await base.UpdateAsync(receiptDetail);

            await UpdateReceiptTotal(receiptDetail.ReceiptId);
            
        }

        // ✅ **Belirli bir fişe ait tüm detayları soft delete ile sil**
        public async Task DeleteByReceiptIdAsync(int receiptId)
        {
            var details = await GetByReceiptIdAsync(receiptId);
            if (details.Any())
            {
                details.ForEach(d => d.IsDeleted = true);
                _dbSet.UpdateRange(details);
                await _context.SaveChangesAsync();
                await UpdateReceiptTotal(receiptId);
            }
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
