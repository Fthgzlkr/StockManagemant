using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class ReceiptDetailManager
    {
        private readonly ReceiptDetailRepository _receiptDetailRepository;
        private readonly ProductRepository _productRepository;
        private readonly ReceiptRepository _receiptRepository;

        public ReceiptDetailManager(ReceiptDetailRepository receiptDetailRepository, ProductRepository productRepository, ReceiptRepository receiptRepository)
        {
            _receiptDetailRepository = receiptDetailRepository;
            _productRepository = productRepository;
            _receiptRepository = receiptRepository;
        }

        // ✅ **Tüm aktif fiş detaylarını getir**
        public async Task<List<ReceiptDetail>> GetAllReceiptDetailsAsync()
        {
            return (await _receiptDetailRepository.GetAllAsync()).ToList();
        }

        // ✅ **Belirli bir fişin aktif detaylarını getir**
        public async Task<List<ReceiptDetail>> GetReceiptDetailsByReceiptIdAsync(int receiptId)
        {
            return await _receiptDetailRepository.GetByReceiptIdAsync(receiptId);
        }

        // ✅ **Fişe yeni ürün ekleme (Toplam tutar otomatik güncellenir)**
        public async Task AddProductToReceiptAsync(int receiptId, int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception("Ürün bulunamadı.");

            // **O anki fiyatı al ve satış fiyatı olarak sabitle**
            decimal productPriceAtSale = product.Price ?? 0;
            decimal subTotal = productPriceAtSale * quantity;

            var receiptDetail = new ReceiptDetail
            {
                ReceiptId = receiptId,
                ProductId = productId,
                Quantity = quantity,
                ProductPriceAtSale = productPriceAtSale,
                SubTotal = subTotal,
                IsDeleted = false // **Yeni eklenen ürün aktif olmalı**
            };

            await _receiptDetailRepository.AddAsync(receiptDetail);

            // ✅ **Fişin toplam tutarını güncelle**
            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

        // ✅ **Fişten ürün kaldırma (Soft Delete)**
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            // **Soft Delete işlemi**
            await _receiptDetailRepository.DeleteAsync(receiptDetailId);

            // ✅ **Fişin toplam tutarını güncelle**
            await _receiptDetailRepository.UpdateReceiptTotal(receiptDetail.ReceiptId);
        }

        // ✅ **Fişteki ürün miktarını güncelleme (Toplam tutar yeniden hesaplanır)**
        public async Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            // ✅ **Yeni miktarı ve toplamı hesapla (Önceden sabitlenen fiyat kullanılıyor)**
            receiptDetail.Quantity = newQuantity;
            receiptDetail.SubTotal = newQuantity * receiptDetail.ProductPriceAtSale;

            await _receiptDetailRepository.UpdateAsync(receiptDetail);

            // ✅ **Fişin toplam tutarını güncelle**
            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

        // ✅ **Belirli fişe ait tüm detayları soft delete ile silme**
        public async Task DeleteDetailsByReceiptIdAsync(int receiptId)
        {
            await _receiptDetailRepository.DeleteByReceiptIdAsync(receiptId);

            // ✅ **Fişin toplam tutarını güncelle**
            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }
    }

}
