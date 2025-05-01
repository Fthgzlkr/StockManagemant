using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using AutoMapper;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
  public class WarehouseProductManager : IWarehouseProductManager
{
    private readonly IWarehouseProductRepository _warehouseProductRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductManager _productManager;
    private readonly IMapper _mapper;
    private readonly IReceiptManager _receiptManager; 
    private readonly IReceiptDetailManager _receiptDetailManager; 
    private readonly IWareHouseLocationRepository _wareHouseLocationRepository;

    public WarehouseProductManager(IWarehouseProductRepository warehouseProductRepository,
                                   IProductRepository productRepository,
                                   IMapper mapper,
                                   IReceiptManager receiptManager,           
                                   IReceiptDetailManager receiptDetailManager,
                                   IWareHouseLocationRepository wareHouseLocationRepository, IProductManager productManager) 
    {
        _warehouseProductRepository = warehouseProductRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _receiptManager = receiptManager; 
        _receiptDetailManager = receiptDetailManager; 
        _wareHouseLocationRepository = wareHouseLocationRepository;
        _productManager = productManager;
    }


        public async Task<List<WarehouseProductDto>> GetProductsByWarehouseIdAsync(int warehouseId)
        {
            var warehouseProducts = await _warehouseProductRepository.GetProductsByWarehouseIdAsync(warehouseId);
            return _mapper.Map<List<WarehouseProductDto>>(warehouseProducts);
        }

        public async Task<int> GetTotalStockByProductIdAsync(int productId)
        {
            return await _warehouseProductRepository.GetTotalStockByProductIdAsync(productId);
        }

        public async Task UpdateStockAsync(UpdateStockDto dto)
        {
            var warehouseProduct = await _warehouseProductRepository.GetByIdAsync(dto.WarehouseProductId);

            if (warehouseProduct == null)
                throw new Exception("Depo ürünü bulunamadı.");

            if (dto.StockQuantity < 0)
                throw new Exception("Stok miktarı negatif olamaz.");

            warehouseProduct.StockQuantity = dto.StockQuantity;

            await _warehouseProductRepository.UpdateAsync(warehouseProduct);
        }    

     public async Task AddProductToWarehouseAsync(WarehouseProductDto dto)
{
    var product = await _productRepository.GetByIdAsync(dto.ProductId);
    if (product == null)
        throw new Exception("Ürün bulunamadı.");

    var existingWarehouseProduct = await _warehouseProductRepository
        .FindAsync(wp => wp.ProductId == dto.ProductId && wp.WarehouseId == dto.WarehouseId);

    if (existingWarehouseProduct.Any())
        throw new Exception("Bu ürün zaten bu depoda mevcut!");

    // 1. Depoya ürünü ekle
    var warehouseProduct = new WarehouseProduct
    {
        ProductId = dto.ProductId,
        WarehouseId = dto.WarehouseId,
        StockQuantity = dto.StockQuantity,
        WarehouseLocationId = dto.WarehouseLocationId,
        StockCode = dto.StockCode,
        IsDeleted = false
    };

    await _warehouseProductRepository.AddAsync(warehouseProduct);

    // ✅ Sadece ürün adı kullanarak açıklama oluşturuyoruz
    string productName = product.Name ?? "Ürün";

    // 2. Giriş Fişi oluştur
    var receiptDto = new ReceiptDto
    {
        WareHouseId = dto.WarehouseId,
        ReceiptType = ReceiptType.Entry, // GİRİŞ fişi olacak
        Description = $"{productName} depoya eklendi.",
    };

    int receiptId = await _receiptManager.AddReceiptAsync(receiptDto);

    // 3. Fiş Detayı oluştur
    await _receiptDetailManager.AddProductToReceiptAsync(receiptId, dto.ProductId, dto.StockQuantity);
}

     public async Task RemoveProductFromWarehouseAsync(int warehouseProductId)
{
    var warehouseProduct = await _warehouseProductRepository.GetByIdAsync(warehouseProductId);
    if (warehouseProduct == null)
        throw new Exception("Depodaki ürün bulunamadı.");

    var product = await _productRepository.GetByIdAsync(warehouseProduct.ProductId);
    if (product == null)
        throw new Exception("İlgili ürün sistemde bulunamadı.");

    // 1. Fiş oluştur (çıkış fişi)
    var receiptDto = new ReceiptDto
    {
        WareHouseId = warehouseProduct.WarehouseId,
        ReceiptType = ReceiptType.Exit,
        Description = $"{product.Name} ürünü depodan kaldırıldı.",
    };

    int receiptId = await _receiptManager.AddReceiptAsync(receiptDto);

    // 2. Fiş detayı oluştur
    await _receiptDetailManager.AddProductToReceiptAsync(receiptId, warehouseProduct.ProductId, warehouseProduct.StockQuantity);

    // 3. Ürünü sil
    await _warehouseProductRepository.DeleteAsync(warehouseProductId);
}

        public async Task RestoreWarehouseProductAsync(int warehouseProductId)
        {
            await _warehouseProductRepository.RestoreAsync(warehouseProductId);
        }

        public async Task<List<WarehouseProductDto>> GetPagedWarehouseProductsAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize)
        {
            var warehouseProducts = await _warehouseProductRepository.GetPagedWarehouseProductsWithStockAsync(filter, warehouseId, page, pageSize);

            return warehouseProducts.Select(wp => new WarehouseProductDto
            {
                WarehouseProductId = wp.Id,
                ProductId = wp.ProductId,
                ProductName = wp.Product.Name,
                CategoryName = wp.Product.Category.Name,
                Price = wp.Product.Price,
                Currency = wp.Product.Currency,
                WarehouseId = wp.WarehouseId,
                WarehouseName = wp.Warehouse.Name,
                StockQuantity = wp.StockQuantity,
                WarehouseLocationId = wp.WarehouseLocationId,
                StockCode = wp.StockCode,
                LocationDisplay = wp.WarehouseLocation != null ? $"{wp.WarehouseLocation.Corridor} > {wp.WarehouseLocation.Shelf} > {wp.WarehouseLocation.Bin}" : null,
                Barcode = wp.Product.Barcode,
                ImageUrl = wp.Product.ImageUrl,
                Description = wp.Product.Description
            }).ToList();
        }

        public async Task<int> GetTotalWarehouseProductCountAsync(WarehouseProductFilter filter,int WarehouseId)
        {
            return await _warehouseProductRepository.GetTotalCountAsync(filter,WarehouseId);
        }

       public async Task<WarehouseProductDto> GetProductInWarehouseByBarcodeAsync(int warehouseId, string barcode)
        {
            var warehouseProduct = await _warehouseProductRepository.GetProductInWarehouseByBarcodeAsync(warehouseId, barcode);

            if (warehouseProduct == null)
                throw new Exception("Ürün bu depoda bulunamadı.");

            return new WarehouseProductDto
            {
                ProductId = warehouseProduct.ProductId,
                ProductName = warehouseProduct.Product.Name,
                Price = warehouseProduct.Product.Price,
                Currency = warehouseProduct.Product.Currency,
                CategoryName = warehouseProduct.Product.Category?.Name,
                WarehouseId = warehouseProduct.WarehouseId,
                WarehouseName = warehouseProduct.Warehouse.Name,
                StockQuantity = warehouseProduct.StockQuantity,
                WarehouseLocationId = warehouseProduct.WarehouseLocationId,
                StockCode = warehouseProduct.StockCode,
                LocationDisplay = warehouseProduct.WarehouseLocation != null
                    ? $"{warehouseProduct.WarehouseLocation.Corridor} > {warehouseProduct.WarehouseLocation.Shelf} > {warehouseProduct.WarehouseLocation.Bin}"
                    : null,
                Barcode = warehouseProduct.Product.Barcode,
                ImageUrl = warehouseProduct.Product.ImageUrl,
                Description = warehouseProduct.Product.Description
            };
        }

public async Task<(int insertedCount, int updatedCount, List<string> errors)> UpsertWarehouseProductsFromExcelAsync(int warehouseId,List<WarehouseProductExcelDto> excelData)
{
    var insertList = new List<WarehouseProduct>();
    var updateList = new List<WarehouseProduct>();
    var errors = new List<string>();

    foreach (var (row, index) in excelData.Select((val, i) => (val, i + 1)))
    {
        if (string.IsNullOrWhiteSpace(row.Barcode))
        {
            errors.Add($"Satır {index}: Barkod boş olamaz.");
            continue;
        }

        var product = await _productRepository.GetProductByBarcodeAsync(row.Barcode);
        if (product == null)
        {
            errors.Add($"Satır {index}: Barkodu '{row.Barcode}' olan ürün bulunamadı.");
            continue;
        }

        bool existsInWarehouse = await _productRepository.IsProductInWarehouseAsync(product.Id, warehouseId);

        if (existsInWarehouse)
        {
            var existing = (await _warehouseProductRepository
                .FindAsync(wp => wp.ProductId == product.Id && wp.WarehouseId == warehouseId && !wp.IsDeleted))
                .FirstOrDefault();

            if (existing == null)
            {
                errors.Add($"Satır {index}: Ürün sistemsel hata nedeniyle depoda bulunamadı.");
                continue;
            }

            if (row.QuantityChange < 0 && existing.StockQuantity + row.QuantityChange < 0)
            {
                errors.Add($"Satır {index}: Stok çıkışı fazla. Mevcut: {existing.StockQuantity}");
                continue;
            }

            existing.StockQuantity += row.QuantityChange;

            if (!string.IsNullOrWhiteSpace(row.StockCode))
                existing.StockCode = row.StockCode;

            updateList.Add(existing);
        }
        else
        {
            if (row.QuantityChange <= 0)
            {
                errors.Add($"Satır {index}: Ürün depoda yokken negatif stok eklenemez.");
                continue;
            }

            var parsed = LocationParser.Parse(row.LocationText);
            if (parsed == null)
            {
                errors.Add($"Satır {index}: Lokasyon formatı hatalı: '{row.LocationText}'");
                continue;
            }

            int? locationId = await _wareHouseLocationRepository.GetLocationIdAsync(
                warehouseId, parsed.Value.corridor, parsed.Value.shelf, parsed.Value.bin);

            if (locationId == null)
            {
                errors.Add($"Satır {index}: Lokasyon bulunamadı: '{row.LocationText}'");
                continue;
            }

            var newWp = new WarehouseProduct
            {
                ProductId = product.Id,
                WarehouseId = warehouseId,
                WarehouseLocationId = locationId.Value,
                StockQuantity = row.QuantityChange,
                StockCode = row.StockCode
            };

            insertList.Add(newWp);
        }
    }

    // 🔁 Güncellemeleri sırayla kaydet
    foreach (var item in updateList)
    {
        await _warehouseProductRepository.UpdateAsync(item);
    }

    // ⏫ Yeni ürünleri topluca ekle
    if (insertList.Any())
    {
        await _warehouseProductRepository.BulkInsertAsync(insertList);
    }

    return (insertList.Count, updateList.Count, errors);
}
    }
}
