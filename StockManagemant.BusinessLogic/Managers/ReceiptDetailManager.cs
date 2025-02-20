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
    public class ReceiptDetailManager
    {
        private readonly ReceiptDetailRepository _receiptDetailRepository;
        private readonly ProductRepository _productRepository;
        private readonly ReceiptRepository _receiptRepository;
        private readonly IMapper _mapper;

        public ReceiptDetailManager(
            ReceiptDetailRepository receiptDetailRepository,
            ProductRepository productRepository,
            ReceiptRepository receiptRepository,
            IMapper mapper)
        {
            _receiptDetailRepository = receiptDetailRepository;
            _productRepository = productRepository;
            _receiptRepository = receiptRepository;
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
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception("Ürün bulunamadı.");

            decimal productPriceAtSale = product.Price ?? 0;
            decimal subTotal = productPriceAtSale * quantity;

            var createDto = new CreateReceiptDetailDto
            {
                ReceiptId = receiptId,
                ProductId = productId,
                Quantity = quantity,
                ProductPriceAtSale = productPriceAtSale,
                SubTotal = subTotal
            };

            var receiptDetail = _mapper.Map<ReceiptDetail>(createDto);
            await _receiptDetailRepository.AddAsync(receiptDetail);

            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

        // ✅ Fişten ürün kaldırma (Soft Delete)
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            await _receiptDetailRepository.DeleteAsync(receiptDetailId);

            await _receiptDetailRepository.UpdateReceiptTotal(receiptDetail.ReceiptId);
        }

        // ✅ Fişteki ürün miktarını güncelleme (DTO kullanımı)
        public async Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            var updateDto = _mapper.Map<UpdateReceiptDetailDto>(receiptDetail);
            updateDto.Quantity = newQuantity;
            updateDto.SubTotal = newQuantity * receiptDetail.ProductPriceAtSale;

            _mapper.Map(updateDto, receiptDetail);
            await _receiptDetailRepository.UpdateAsync(receiptDetail);

            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }

        // ✅ Belirli fişe ait tüm detayları soft delete ile silme
        public async Task DeleteDetailsByReceiptIdAsync(int receiptId)
        {
            await _receiptDetailRepository.DeleteByReceiptIdAsync(receiptId);

            await _receiptDetailRepository.UpdateReceiptTotal(receiptId);
        }
    }
}
