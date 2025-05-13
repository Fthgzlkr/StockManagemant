using AutoMapper;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using StockManagemant.Entities.Enums;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
    public class ReceiptManager:IReceiptManager
    {
        private readonly IReceiptRepository _receiptRepository;
        private readonly IReceiptDetailManager _receiptDetailManager;
        private readonly IReceiptDetailRepository _receiptDetailRepository;
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWarehouseProductRepository _warehouseProductRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public ReceiptManager(IReceiptRepository receiptRepository, IReceiptDetailManager receiptDetailManager, IReceiptDetailRepository receiptDetailRepository, IMapper mapper, IWarehouseProductRepository warehouseProductRepository,IWarehouseRepository warehouseRepository, ICustomerRepository customerRepository)
        {
            _receiptRepository = receiptRepository;
            _receiptDetailManager = receiptDetailManager;
            _receiptDetailRepository = receiptDetailRepository;
            _mapper = mapper;
            _warehouseProductRepository = warehouseProductRepository;
            _warehouseRepository = warehouseRepository;
            _customerRepository=customerRepository;
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

        public async Task<int> AddReceiptAsync(ReceiptDto receiptDto)
        {
            if (receiptDto == null || receiptDto.WareHouseId == 0)
                throw new Exception("Hata: Depo ID boş olamaz!");

            var receipt = new Receipt
            {
                Date = receiptDto.Date,
                TotalAmount = 0,
                IsDeleted = false,
                WarehouseId = receiptDto.WareHouseId,
                ReceiptType = receiptDto.ReceiptType,
                Description = receiptDto.Description,
                SourceType=receiptDto.SourceType,
                SourceId=receiptDto.SourceId,
            };

            return await _receiptRepository.AddReceiptAsync(receipt);
        }

        public async Task UpdateReceiptDateAsync(ReceiptDto updateDto)
        {
            var receipt = await _receiptRepository.GetByIdAsync(updateDto.Id ?? 0);
            if (receipt == null) throw new Exception("Fiş bulunamadı!");

            // Güncelleme işlemi
            receipt.Date = updateDto.Date;
            receipt.TotalAmount = updateDto.TotalAmount;

            await _receiptRepository.UpdateAsync(receipt);
        }

        // ✅ **Fiş güncelle (Toplam tutarı da günceller)**
        public async Task UpdateReceiptAsync(int receiptId)
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            if (receipt == null) throw new Exception("Fiş bulunamadı.");

            // Fişe bağlı detaylardan toplam tutarı hesapla
            receipt.TotalAmount = await _receiptDetailRepository.GetTotalAmountByReceiptIdAsync(receiptId);

            var receiptDto = _mapper.Map<ReceiptDto>(receipt);
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
    if (receipt == null) return null;

    var receiptDto = _mapper.Map<ReceiptDto>(receipt);

    // 🔥 Ek: Warehouse adını çekip ReceiptDto'ya ekliyoruz
    var warehouse = await _warehouseRepository.GetByIdAsync(receipt.WarehouseId);
    receiptDto.WareHouseName = warehouse?.Name ?? "Depo Adı Yok";

    return receiptDto;
}

        // ✅ **Fişin ürün detaylarını getirme**
        public async Task<List<ReceiptDetailDto>> GetReceiptDetailsAsync(int receiptId)
        {
            var receiptDetails = await _receiptDetailManager.GetReceiptDetailsByReceiptIdAsync(receiptId);
            return _mapper.Map<List<ReceiptDetailDto>>(receiptDetails);
        }

       

        // ✅ **Fişten ürün kaldırma (Toplam tutar güncellenir)**
        public async Task RemoveProductFromReceiptAsync(int receiptDetailId)
        {
            var receiptDetail = await _receiptDetailRepository.GetByIdAsync(receiptDetailId);
            if (receiptDetail == null) throw new Exception("Fiş detayı bulunamadı.");

            int receiptId = receiptDetail.ReceiptId;

            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            if (receipt == null) throw new Exception("Fiş bulunamadı.");

            var warehouseProducts = await _warehouseProductRepository.GetProductsByWarehouseIdAsync(receipt.WarehouseId);
            var targetWarehouseProduct = warehouseProducts.FirstOrDefault(wp => wp.ProductId == receiptDetail.ProductId);
            if (targetWarehouseProduct == null) throw new Exception("Depo ürünü bulunamadı.");

            if (receipt.ReceiptType == ReceiptType.Entry)
            {
                targetWarehouseProduct.StockQuantity -= receiptDetail.Quantity;
                if (targetWarehouseProduct.StockQuantity < 0)
                    throw new Exception("Stok yetersiz. Çıkarılan miktar stoktan fazla.");
            }
            else if (receipt.ReceiptType == ReceiptType.Exit)
            {
                targetWarehouseProduct.StockQuantity += receiptDetail.Quantity;
            }

            await _warehouseProductRepository.UpdateAsync(targetWarehouseProduct);

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
