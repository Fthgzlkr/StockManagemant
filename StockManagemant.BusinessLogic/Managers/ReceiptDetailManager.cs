using AutoMapper;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
    public class ReceiptDetailManager : IReceiptDetailManager
    {
        private readonly IReceiptDetailRepository _receiptDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseProductRepository _warehouseProductRepository;
        private readonly IReceiptRepository _receiptRepository;

        private readonly IMapper _mapper;

        public ReceiptDetailManager(
            IReceiptDetailRepository receiptDetailRepository,
            IProductRepository productRepository,
            IReceiptRepository receiptRepository,
            IWarehouseProductRepository warehouseProductRepository,
            IMapper mapper)
        {
            _receiptDetailRepository = receiptDetailRepository;
            _productRepository = productRepository;
            _receiptRepository = receiptRepository;
            _warehouseProductRepository = warehouseProductRepository;

            _mapper = mapper;
        }

        // Tüm aktif fiş detaylarını getir 
        public async Task<List<ReceiptDetailDto>> GetAllReceiptDetailsAsync()
        {
            var receiptDetails = await _receiptDetailRepository.GetAllAsync();
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

        //  Belirli bir fişin aktif detaylarını getir 
        public async Task<List<ReceiptDetailDto>> GetReceiptDetailsByReceiptIdAsync(int receiptId)
        {
            var receiptDetails = await _receiptDetailRepository.GetByReceiptIdAsync(receiptId);
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

        //  Fişe yeni ürün ekleme 
        public async Task AddProductToReceiptAsync(int receiptId, int productId, int quantity)
        {
            if (quantity <= 0) throw new Exception("Hata: Ürün miktarı sıfırdan büyük olmalıdır!");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception("Hata: Ürün bulunamadı!");

            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            if (receipt == null) throw new Exception("Hata: Fiş bulunamadı!");

            var warehouseProduct = await _warehouseProductRepository.GetProductInWarehouseByBarcodeAsync(receipt.WarehouseId, product.Barcode);
            if (warehouseProduct == null) throw new Exception("Bu ürün ilgili depoda bulunamadı!");

            // Stok güncellemesi
            if (receipt.ReceiptType == ReceiptType.Entry)
            {
                warehouseProduct.StockQuantity += quantity;
            }
            else if (receipt.ReceiptType == ReceiptType.Exit)
            {
                warehouseProduct.StockQuantity -= quantity;
                if (warehouseProduct.StockQuantity < 0)
                    throw new Exception("Yetersiz stok! Stok miktarı sıfırın altına inemez.");
            }

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
            await _warehouseProductRepository.UpdateAsync(warehouseProduct);
            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

// ReceiptDetailManager'a bu metodu ekleyin:
public async Task AddReceiptDetailOnlyAsync(int receiptId, int productId, int quantity)
{
    // Validasyonlar
    if (quantity <= 0) 
        throw new Exception("Hata: Ürün miktarı sıfırdan büyük olmalıdır!");

    var product = await _productRepository.GetByIdAsync(productId);
    if (product == null) 
        throw new Exception("Hata: Ürün bulunamadı!");

    var receipt = await _receiptRepository.GetByIdAsync(receiptId);
    if (receipt == null) 
        throw new Exception("Hata: Fiş bulunamadı!");

    // Ürün fiyatını al
    decimal productPriceAtSale = product.Price ?? 0;

    // SADECE Fiş Detayı oluştur - Stok işlemi YAPMA
    var receiptDetail = new ReceiptDetail
    {
        ReceiptId = receiptId,
        ProductId = productId,
        Quantity = quantity,
        ProductPriceAtSale = productPriceAtSale,
        SubTotal = quantity * productPriceAtSale
    };

    // Fiş detayını kaydet
    await _receiptDetailRepository.AddAsync(receiptDetail);
    
    // Fiş toplamını güncelle
    await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
}
        // Fişten ürün kaldırma 
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            var receipt = await _receiptRepository.GetByIdAsync(receiptDetail.ReceiptId);
            if (receipt == null) throw new Exception("Fiş bulunamadı.");

            var product = await _productRepository.GetByIdAsync(receiptDetail.ProductId);
            if (product == null) throw new Exception("Ürün bulunamadı.");

            var warehouseProduct = await _warehouseProductRepository.GetProductInWarehouseByBarcodeAsync(receipt.WarehouseId, product.Barcode);
            if (warehouseProduct == null) throw new Exception("Ürün depoda bulunamadı.");

            // Stok geri iade işlemi
            if (receipt.ReceiptType == ReceiptType.Entry)
            {
                warehouseProduct.StockQuantity -= receiptDetail.Quantity;
                if (warehouseProduct.StockQuantity < 0)
                    throw new Exception("Stok miktarı sıfırın altına inemez!");
            }
            else if (receipt.ReceiptType == ReceiptType.Exit)
            {
                warehouseProduct.StockQuantity += receiptDetail.Quantity;
            }

            await _warehouseProductRepository.UpdateAsync(warehouseProduct);
            await _receiptDetailRepository.DeleteAsync(receiptDetailId);
            await _receiptDetailRepository.UpdateReceiptTotal(receiptDetail.ReceiptId);
        }

        //  Fişteki ürün miktarını güncelleme 
        public async Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            var receipt = await _receiptRepository.GetByIdAsync(receiptDetail.ReceiptId);
            if (receipt == null) throw new Exception("Fiş bulunamadı.");

            var product = await _productRepository.GetByIdAsync(receiptDetail.ProductId);
            if (product == null) throw new Exception("Ürün bulunamadı.");

            var warehouseProduct = await _warehouseProductRepository.GetProductInWarehouseByBarcodeAsync(receipt.WarehouseId, product.Barcode);
            if (warehouseProduct == null) throw new Exception("Ürün depoda bulunamadı.");

            int quantityDifference = newQuantity - receiptDetail.Quantity;

            if (receipt.ReceiptType == ReceiptType.Entry)
            {
                warehouseProduct.StockQuantity += quantityDifference;
            }
            else if (receipt.ReceiptType == ReceiptType.Exit)
            {
                warehouseProduct.StockQuantity -= quantityDifference;
                if (warehouseProduct.StockQuantity < 0)
                {
                    throw new Exception("Yetersiz stok! Stok miktarı sıfırın altına inemez.");
                }
            }

            receiptDetail.Quantity = newQuantity;
            receiptDetail.SubTotal = newQuantity * receiptDetail.ProductPriceAtSale;

            await _receiptDetailRepository.UpdateAsync(receiptDetail);
            await _warehouseProductRepository.UpdateAsync(warehouseProduct);
            await _receiptDetailRepository.UpdateReceiptTotal(receiptDetail.ReceiptId);
        }

        // Belirli fişe ait tüm detayları soft delete ile silme
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
