using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using AutoMapper;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Enums;

namespace StockManagemant.Business.Managers
{
    public class WarehouseProductManager : IWarehouseProductManager
    {
        private readonly IWarehouseProductRepository _warehouseProductRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductManager _productManager;
        private readonly IMapper _mapper;
        private readonly IReceiptManager _receiptManager;
        private readonly IReceiptDetailManager _receiptDetailManager;
        private readonly IWareHouseLocationRepository _wareHouseLocationRepository;
        private readonly ILogManager _logManager;

        public WarehouseProductManager(IWarehouseProductRepository warehouseProductRepository,
                                       IProductRepository productRepository,
                                       IMapper mapper,
                                       IReceiptManager receiptManager,
                                       IReceiptDetailManager receiptDetailManager,
                                       IWareHouseLocationRepository wareHouseLocationRepository, IProductManager productManager,
                                      IWarehouseRepository warehouseRepository,
                                      ILogManager logManager)
        {
            _warehouseProductRepository = warehouseProductRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _receiptManager = receiptManager;
            _receiptDetailManager = receiptDetailManager;
            _wareHouseLocationRepository = wareHouseLocationRepository;
            _productManager = productManager;
            _warehouseRepository = warehouseRepository;
            _logManager = logManager;
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
                SourceType = ReceiptSourceType.None,
                SourceId = null,
                SourceName = "Depo giriş sağlayıcıs",
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
                SourceType = ReceiptSourceType.None,
                SourceId = null,
                SourceName = "Depodan Çıkış",
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

        public async Task<int> GetTotalWarehouseProductCountAsync(WarehouseProductFilter filter, int WarehouseId)
        {
            return await _warehouseProductRepository.GetTotalCountAsync(filter, WarehouseId);
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

        private async Task LogExcelErrorAsync(int rowIndex, string message, string? target, string batchId, string fileName, int userId)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Excel Depo Ürün Yükleme",
                Message = $"Satır {rowIndex}: {message}",
                Level = "Error",
                FileName = fileName,
                BatchId = batchId,
                Target = target,
                Timestamp = DateTime.Now
            });
        }

        public async Task<(int insertedCount, int updatedCount, List<string> errors)> UpsertWarehouseProductsFromExcelAsync(int warehouseId, List<WarehouseProductExcelDto> excelData, string fileName, int userId)
        {
            var batchId = Guid.NewGuid().ToString();
            var insertList = new List<WarehouseProduct>();
            var updateList = new List<WarehouseProduct>();
            var receiptEntryDetails = new List<(int ProductId, int Quantity)>();
            var receiptExitDetails = new List<(int ProductId, int Quantity)>();
            var errors = new List<string>();

            foreach (var (row, index) in excelData.Select((val, i) => (val, i + 1)))
            {
                if (string.IsNullOrWhiteSpace(row.Barcode))
                {
                    var errorMsg = "Barkod boş olamaz.";
                    errors.Add($"Satır {index}: {errorMsg}");
                    await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                    continue;
                }

                var product = await _productRepository.GetProductByBarcodeAsync(row.Barcode);
                if (product == null)
                {
                    var errorMsg = $"Barkodu '{row.Barcode}' olan ürün bulunamadı.";
                    errors.Add($"Satır {index}: {errorMsg}");
                    await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
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
                        var errorMsg = "Ürün sistemsel hata nedeniyle depoda bulunamadı.";
                        errors.Add($"Satır {index}: {errorMsg}");
                        await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                        continue;
                    }

                    if (row.QuantityChange < 0 && existing.StockQuantity + row.QuantityChange < 0)
                    {
                        var errorMsg = $"Stok çıkışı fazla. Mevcut: {existing.StockQuantity}";
                        errors.Add($"Satır {index}: {errorMsg}");
                        await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                        continue;
                    }

                    existing.StockQuantity += row.QuantityChange;

                    if (!string.IsNullOrWhiteSpace(row.StockCode))
                        existing.StockCode = row.StockCode;

                    updateList.Add(existing);

                    if (row.QuantityChange > 0)
                        receiptEntryDetails.Add((existing.ProductId, row.QuantityChange));
                    else if (row.QuantityChange < 0)
                        receiptExitDetails.Add((existing.ProductId, Math.Abs(row.QuantityChange)));
                }
                else
                {
                    if (row.QuantityChange <= 0)
                    {
                        var errorMsg = "Ürün depoda yokken negatif stok eklenemez.";
                        errors.Add($"Satır {index}: {errorMsg}");
                        await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                        continue;
                    }

                    var parsed = LocationParser.Parse(row.LocationText);
                    if (parsed == null)
                    {
                        var errorMsg = $"Lokasyon formatı hatalı: '{row.LocationText}'";
                        errors.Add($"Satır {index}: {errorMsg}");
                        await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                        continue;
                    }

                    int? locationId = await _wareHouseLocationRepository.GetLocationIdAsync(
                        warehouseId, parsed.Value.corridor, parsed.Value.shelf, parsed.Value.bin);

                    if (locationId == null)
                    {
                        var errorMsg = $"Lokasyon bulunamadı: '{row.LocationText}'";
                        errors.Add($"Satır {index}: {errorMsg}");
                        await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
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
                    receiptEntryDetails.Add((newWp.ProductId, row.QuantityChange));
                }
            }

            foreach (var item in updateList)
            {
                await _warehouseProductRepository.UpdateAsync(item);
            }

            if (insertList.Any())
            {
                await _warehouseProductRepository.BulkInsertAsync(insertList);
            }

            // 📋 Fiş oluşturma işlemi
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            string warehouseName = warehouse?.Name ?? "Depo";

            if (receiptEntryDetails.Any())
            {
                var receipt = new ReceiptDto
                {
                    WareHouseId = warehouseId,
                    ReceiptType = ReceiptType.Entry,
                    Description = $"{warehouseName} deposuna Excel dosyasıyla toplu ürün girişi yapıldı."
                };

                int receiptId = await _receiptManager.AddReceiptAsync(receipt);
                foreach (var item in receiptEntryDetails)
                {
                    await _receiptDetailManager.AddProductToReceiptAsync(receiptId, item.ProductId, item.Quantity);
                }
            }

            if (receiptExitDetails.Any())
            {
                var receipt = new ReceiptDto
                {
                    WareHouseId = warehouseId,
                    ReceiptType = ReceiptType.Exit,
                    Description = $"{warehouseName} deposundan Excel dosyasıyla toplu ürün çıkışı yapıldı."
                };

                int receiptId = await _receiptManager.AddReceiptAsync(receipt);
                foreach (var item in receiptExitDetails)
                {
                    await _receiptDetailManager.AddProductToReceiptAsync(receiptId, item.ProductId, item.Quantity);
                }
            }

            return (insertList.Count, updateList.Count, errors);
        }

public async Task EnsureWarehouseProductExistsAsync(int warehouseId, int productId)
{
    var existing = await _warehouseProductRepository
        .FindAsync(wp => wp.ProductId == productId && wp.WarehouseId == warehouseId);

    if (!existing.Any())
    {
        var newWarehouseProduct = new WarehouseProduct
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            StockQuantity = 0,
            IsDeleted = false
        };

        await _warehouseProductRepository.AddAsync(newWarehouseProduct);
    }
}
    }
}
