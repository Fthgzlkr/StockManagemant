using AutoMapper;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class ReceiptManager
    {
        private readonly ReceiptRepository _receiptRepository;
        private readonly ReceiptDetailManager _receiptDetailManager;
        private readonly ReceiptDetailRepository _receiptDetailRepository;
        private readonly IMapper _mapper;

        public ReceiptManager(ReceiptRepository receiptRepository, ReceiptDetailManager receiptDetailManager, ReceiptDetailRepository receiptDetailRepository,IMapper mapper)
        {
            _receiptRepository = receiptRepository;
            _receiptDetailManager = receiptDetailManager;
            _receiptDetailRepository = receiptDetailRepository;
            _mapper = mapper;
        }

        public async Task<int> GetTotalReceiptCountAsync(ReceiptFilter filter)
        {
          
            return await _receiptRepository.GetTotalCountAsync(filter);
        }

        // ✅ **Sayfalama ile fişleri getir**
        public async Task<List<ReceiptDto>> GetPagedReceiptAsync(int page, int pageSize, ReceiptFilter filter)
        {
            var receipts = await _receiptRepository.GetPagedReceiptsAsync(filter, page, pageSize);
            return _mapper.Map<List<ReceiptDto>>(receipts);
        }



        // ✅ **Yeni fiş ekle (TotalAmount = 0 başlangıç değeri ile)**
        public async Task<int> AddReceiptAsync()
        {
            var receiptDto = new CreateReceiptDto
            {
                TotalAmount = 0
            };

            var receipt = _mapper.Map<Receipt>(receiptDto);
            return await _receiptRepository.AddReceiptAsync(receipt);
        }


        // ✅ **Fiş güncelle (Toplam tutarı da günceller)**
        public async Task UpdateReceiptAsync(int receiptId)
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            if (receipt == null) throw new Exception("Fiş bulunamadı.");

            // Fişe bağlı detaylardan toplam tutarı hesapla
            receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receiptId);

            var receiptDto = _mapper.Map<UpdateReceiptDto>(receipt);
            _mapper.Map(receiptDto, receipt);

            await _receiptRepository.UpdateAsync(receipt);
        }

        // ✅ **Fiş silme (Önce detayları soft delete yapar, sonra fişi soft delete yapar)**
        public async Task DeleteReceiptAsync(int receiptId)
        {
            // **Fişe bağlı tüm detayları soft delete ile işaretle**

            Console.WriteLine("Silme işlemi başladı. ID: " + receiptId);
            await _receiptDetailManager.DeleteDetailsByReceiptIdAsync(receiptId);

            // **Fişi de soft delete yap**
            await _receiptRepository.DeleteAsync(receiptId);
        }

        // ✅ **ID’ye göre fiş bulma**
        public async Task<ReceiptDto> GetReceiptByIdAsync(int receiptId)
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            return _mapper.Map<ReceiptDto>(receipt);
        }

        // ✅ **Fişin ürün detaylarını getirme**
        public async Task<List<ReceiptDetailDto>> GetReceiptDetailsAsync(int receiptId)
        {
            var receiptDetails = await _receiptDetailManager.GetReceiptDetailsByReceiptIdAsync(receiptId);
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

        // ✅ **Fişe ürün ekleme (Toplam tutar otomatik güncellenir)**
        public async Task AddProductToReceiptAsync(int receiptId, int productId, int quantity)
        {
            await _receiptDetailManager.AddProductToReceiptAsync(receiptId, productId, quantity);

            // Fişin toplam tutarını güncelle
            await UpdateReceiptAsync(receiptId);
        }

        // ✅ **Fişten ürün kaldırma (Toplam tutar güncellenir)**
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            await _receiptDetailManager.RemoveProductFromReceiptAsync(receiptDetailId);

            // **Fişin toplam tutarını güncelle**
            await UpdateReceiptAsync(receiptId);
        }

        // ✅ **Fişteki ürün miktarını güncelleme (Toplam tutar güncellenir)**
        public async Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            await _receiptDetailManager.UpdateProductQuantityInReceiptAsync(receiptDetailId, newQuantity);

            await UpdateReceiptAsync(receiptId);
        }



   }

}
