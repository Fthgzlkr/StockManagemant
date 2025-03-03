using AutoMapper;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StockManagemant.Business.Managers
{
    public class ReceiptDetailManager :IReceiptDetailManager
    {
        private readonly ReceiptDetailRepository _receiptDetailRepository;
        private readonly ProductRepository _productRepository;
        private readonly WarehouseProductRepository _warehouseProductRepository;
      
        private readonly IMapper _mapper;

        public ReceiptDetailManager(
            ReceiptDetailRepository receiptDetailRepository,
            ProductRepository productRepository,
            ReceiptRepository receiptRepository,
            WarehouseProductRepository warehouseProductRepository,
            IMapper mapper)
        {
            _receiptDetailRepository = receiptDetailRepository;
            _productRepository = productRepository;
            _warehouseProductRepository = warehouseProductRepository;
            
            _mapper = mapper;
        }

        // ✅ Tüm aktif fiş detaylarını getir (DTO kullanımı)
        public async Task<List<ReceiptDetailDto>> GetAllReceiptDetailsAsync()
        {
            var receiptDetails = await _receiptDetailRepository.GetAllAsync();
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

        // ✅ Belirli bir fişin aktif detaylarını getir (DTO kullanımı)
        public async Task<List<ReceiptDetailDto>> GetReceiptDetailsByReceiptIdAsync(int receiptId)
        {
            var receiptDetails = await _receiptDetailRepository.GetByReceiptIdAsync(receiptId);
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

        // ✅ Fişe yeni ürün ekleme (DTO kullanımı)
        public async Task AddProductToReceiptAsync(int receiptId, int productId, int quantity)
        {
            if (quantity <= 0) throw new Exception("Hata: Ürün miktarı sıfırdan büyük olmalıdır!");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception("Hata: Ürün bulunamadı!");

            decimal productPriceAtSale = product.Price ?? 0;

            var receiptDetail = new ReceiptDetail
            {
                ReceiptId = receiptId,
                ProductId = productId,
                Quantity = quantity,
                ProductPriceAtSale = productPriceAtSale,
                SubTotal = quantity * productPriceAtSale
            };

            await _receiptDetailRepository.AddAsync(receiptDetail);
            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }



        // ✅ Fişten ürün kaldırma (Soft Delete)
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            await _receiptDetailRepository.DeleteAsync(receiptDetailId); // ✅ Generic Repository DeleteAsync kullanıldı

            await _receiptDetailRepository.UpdateReceiptTotal(receiptDetail.ReceiptId);
        }


        // ✅ Fişteki ürün miktarını güncelleme (DTO kullanımı)
        public async Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            
            var warehouseProduct = await _warehouseProductRepository.GetByIdAsync(receiptDetail.ProductId);
            if (warehouseProduct == null) throw new Exception("Ürün depoda bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            
            int quantityDifference = newQuantity - receiptDetail.Quantity;

            
            warehouseProduct.StockQuantity -= quantityDifference;

            if (warehouseProduct.StockQuantity < 0)
            {
                throw new Exception("Yetersiz stok! Stok miktarı sıfırın altına inemez.");
            }

            
            receiptDetail.Quantity = newQuantity;
            receiptDetail.SubTotal = newQuantity * receiptDetail.ProductPriceAtSale;

           
            var updateWarehouseProductDto = _mapper.Map<UpdateWarehouseProductStockDto>(warehouseProduct);

          
            await _receiptDetailRepository.UpdateAsync(receiptDetail);
            await _warehouseProductRepository.UpdateAsync(warehouseProduct); 

            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }



        // ✅ Belirli fişe ait tüm detayları soft delete ile silme
        public async Task DeleteDetailsByReceiptIdAsync(int receiptId)
        {
            var details = await _receiptDetailRepository.GetByReceiptIdAsync(receiptId);

            foreach (var detail in details)
            {
                await _receiptDetailRepository.DeleteAsync(detail.Id); // ✅ Generic Repository DeleteAsync kullanıldı
            }

            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

    }
}
