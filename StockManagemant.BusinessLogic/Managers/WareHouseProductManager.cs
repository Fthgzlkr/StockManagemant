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
    // Geçici çözüm: UserId'yi 0 olarak set et veya DTO'dan al
    int userId = dto.UserId > 0 ? dto.UserId : 0; // Şimdilik default 0

    var warehouseProduct = await _warehouseProductRepository.GetByIdAsync(dto.WarehouseProductId);

    if (warehouseProduct == null)
    {
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Stok Güncelleme",
            Message = $"Depo ürünü bulunamadı. ID: {dto.WarehouseProductId}",
            Level = "Error",
            Target = dto.WarehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });
        throw new Exception("Depo ürünü bulunamadı.");
    }

    if (dto.StockQuantity < 0)
    {
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Stok Güncelleme",
            Message = $"Negatif stok girişi engellendi. Girilen değer: {dto.StockQuantity}",
            Level = "Warning",
            Target = dto.WarehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });
        throw new Exception("Stok miktarı negatif olamaz.");
    }

    var oldQuantity = warehouseProduct.StockQuantity;
    warehouseProduct.StockQuantity = dto.StockQuantity;

    await _warehouseProductRepository.UpdateAsync(warehouseProduct);

    // Başarılı güncelleme logu
    await _logManager.LogAsync(new AppLogDto
    {
        UserId = userId,
        Action = "Stok Güncelleme",
        Message = $"Stok güncellendi. Eski: {oldQuantity}, Yeni: {dto.StockQuantity}",
        Level = "Info",
        Target = dto.WarehouseProductId.ToString(),
        Timestamp = DateTime.Now
    });

    // Düşük stok kontrolü
    await CheckAndLogLowStock(warehouseProduct, userId);
}

   public async Task AddProductToWarehouseAsync(WarehouseProductDto dto)
{
    int userId = dto.UserId > 0 ? dto.UserId : 0;

    try
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depoya Ürün Ekleme",
                Message = $"Ürün bulunamadı. Ürün ID: {dto.ProductId}",
                Level = "Error",
                Target = $"Product:{dto.ProductId}",
                Timestamp = DateTime.Now
            });
            throw new Exception("Ürün bulunamadı.");
        }

        var existingWarehouseProduct = await _warehouseProductRepository
            .FindAsync(wp => wp.ProductId == dto.ProductId && wp.WarehouseId == dto.WarehouseId);

        if (existingWarehouseProduct.Any())
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depoya Ürün Ekleme",
                Message = $"Ürün zaten depoda mevcut. Ürün: {product.Name}, Depo ID: {dto.WarehouseId}",
                Level = "Warning",
                Target = $"Product:{dto.ProductId}, Warehouse:{dto.WarehouseId}",
                Timestamp = DateTime.Now
            });
            throw new Exception("Bu ürün zaten bu depoda mevcut!");
        }

        var location = await _wareHouseLocationRepository.GetLocationByIdAsync(dto.WarehouseLocationId ?? 0);
        if (location == null)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depoya Ürün Ekleme",
                Message = $"Lokasyon bulunamadı. Lokasyon ID: {dto.WarehouseLocationId}",
                Level = "Error",
                Target = $"Location:{dto.WarehouseLocationId}",
                Timestamp = DateTime.Now
            });
            throw new Exception("Lokasyon bilgisi alınamadı.");
        }

        if (product.StorageType != StorageType.Undefined &&
            location.StorageType != StorageType.Undefined &&
            product.StorageType != location.StorageType)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depoya Ürün Ekleme",
                Message = $"Depolama tipi uyuşmazlığı. Ürün: {product.StorageType}, Lokasyon: {location.StorageType}",
                Level = "Error",
                Target = $"Product:{dto.ProductId}, Location:{dto.WarehouseLocationId}",
                Timestamp = DateTime.Now
            });
            throw new Exception($"Depolama tipi uyuşmazlığı. Ürün: {product.StorageType}, Lokasyon: {location.StorageType}");
        }

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

        string productName = product.Name ?? "Ürün";

        // 2. Giriş Fişi oluştur
        var receiptDto = new ReceiptDto
        {
            WareHouseId = dto.WarehouseId,
            ReceiptType = ReceiptType.Entry,
            SourceType = ReceiptSourceType.None,
            SourceId = null,
            SourceName = "Depo giriş sağlayıcısı",
            Description = $"{productName} depoya eklendi.",
        };

        int receiptId = await _receiptManager.AddReceiptAsync(receiptDto);

        // 3. SADECE Fiş Detayı oluştur (stok hareket işlemi yapma)
        await _receiptDetailManager.AddReceiptDetailOnlyAsync(receiptId, dto.ProductId, dto.StockQuantity);

        // Başarılı ekleme logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Depoya Ürün Ekleme",
            Message = $"Ürün depoya eklendi. Ürün: {productName}, Miktar: {dto.StockQuantity}, Fiş ID: {receiptId}",
            Level = "Info",
            Target = $"Product:{dto.ProductId}, Warehouse:{dto.WarehouseId}",
            Timestamp = DateTime.Now
        });

        // Düşük stok kontrolü
        await CheckAndLogLowStock(warehouseProduct, userId);
    }
    catch (Exception ex)
    {
        if (!ex.Message.Contains("bulunamadı") && !ex.Message.Contains("mevcut") && !ex.Message.Contains("uyuşmazlığı"))
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depoya Ürün Ekleme",
                Message = $"Beklenmeyen hata: {ex.Message}",
                Level = "Error",
                Target = $"Product:{dto.ProductId}, Warehouse:{dto.WarehouseId}",
                Timestamp = DateTime.Now
            });
        }
        throw;
    }
}
// Manager class'ının başına sabit ekle
private const int LOW_STOCK_THRESHOLD = 20;

// Düşük stok kontrolü için yardımcı metod
private async Task CheckAndLogLowStock(WarehouseProduct warehouseProduct, int userId)
{
    // Sadece 20'nin altındaki stoklar için uyarı
    if (warehouseProduct.StockQuantity < LOW_STOCK_THRESHOLD)
    {
        // Ürün ve depo bilgilerini al
        var product = await _productRepository.GetByIdAsync(warehouseProduct.ProductId);
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseProduct.WarehouseId);

        string productName = product?.Name ?? "Bilinmeyen Ürün";
        string warehouseName = warehouse?.Name ?? "Bilinmeyen Depo";

        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Düşük Stok Uyarısı",
            Message = $"Düşük stok tespit edildi! Ürün: {productName}, " +
                     $"Depo: {warehouseName}, " +
                     $"Mevcut Stok: {warehouseProduct.StockQuantity}, " +
                     $"Limit: {LOW_STOCK_THRESHOLD}",
            Level = "Warning",
            Target = $"Product:{warehouseProduct.ProductId}, Warehouse:{warehouseProduct.WarehouseId}",
            Timestamp = DateTime.Now
        });
    }
}


       public async Task RemoveProductFromWarehouseAsync(int warehouseProductId)
{
    int userId = 0; // Şimdilik default değer, sonradan authentication'dan alınacak
    
    try
    {
        // DEBUG: Başlangıç logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Depodan Ürün Çıkarma Başlangıç",
            Message = $"Ürün çıkarma işlemi başlatıldı. WarehouseProductId: {warehouseProductId}",
            Level = "Info",
            Target = warehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });

        var warehouseProduct = await _warehouseProductRepository.GetByIdAsync(warehouseProductId);
        if (warehouseProduct == null)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depodan Ürün Çıkarma",
                Message = $"Depodaki ürün bulunamadı. WarehouseProductId: {warehouseProductId}",
                Level = "Error",
                Target = warehouseProductId.ToString(),
                Timestamp = DateTime.Now
            });
            throw new Exception("Depodaki ürün bulunamadı.");
        }

        var product = await _productRepository.GetByIdAsync(warehouseProduct.ProductId);
        if (product == null)
        {
            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Depodan Ürün Çıkarma",
                Message = $"İlgili ürün sistemde bulunamadı. ProductId: {warehouseProduct.ProductId}",
                Level = "Error",
                Target = $"Product:{warehouseProduct.ProductId}",
                Timestamp = DateTime.Now
            });
            throw new Exception("İlgili ürün sistemde bulunamadı.");
        }

        // DEBUG: Ürün bilgileri logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Ürün Bilgileri",
            Message = $"Çıkarılacak ürün: {product.Name}, Mevcut Stok: {warehouseProduct.StockQuantity}, Depo: {warehouseProduct.WarehouseId}",
            Level = "Info",
            Target = $"Product:{warehouseProduct.ProductId}, Warehouse:{warehouseProduct.WarehouseId}",
            Timestamp = DateTime.Now
        });

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

        // DEBUG: Fiş oluşturuldu logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Çıkış Fişi Oluşturuldu",
            Message = $"Çıkış fişi oluşturuldu. ReceiptId: {receiptId}",
            Level = "Info",
            Target = $"Receipt:{receiptId}",
            Timestamp = DateTime.Now
        });

        // 2. Fiş detayı oluştur
        await _receiptDetailManager.AddProductToReceiptAsync(receiptId, warehouseProduct.ProductId, warehouseProduct.StockQuantity);

        // DEBUG: Fiş detayı oluşturuldu logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Fiş Detayı Oluşturuldu",
            Message = $"Fiş detayı oluşturuldu. ProductId: {warehouseProduct.ProductId}, Quantity: {warehouseProduct.StockQuantity}",
            Level = "Info",
            Target = $"Receipt:{receiptId}, Product:{warehouseProduct.ProductId}",
            Timestamp = DateTime.Now
        });

        // 3. Ürünü sil
        await _warehouseProductRepository.DeleteAsync(warehouseProductId);

        // Başarılı çıkarma logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Depodan Ürün Çıkarma",
            Message = $"Ürün depodan başarıyla çıkarıldı. Ürün: {product.Name}, Çıkarılan Miktar: {warehouseProduct.StockQuantity}, Fiş ID: {receiptId}",
            Level = "Info",
            Target = $"Product:{warehouseProduct.ProductId}, Warehouse:{warehouseProduct.WarehouseId}",
            Timestamp = DateTime.Now
        });

        // DEBUG: İşlem tamamlandı logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Ürün Çıkarma Tamamlandı",
            Message = $"Ürün çıkarma işlemi başarıyla tamamlandı. WarehouseProductId: {warehouseProductId}",
            Level = "Info",
            Target = warehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        // Genel hata logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "Depodan Ürün Çıkarma",
            Message = $"Ürün çıkarma hatası: {ex.Message}",
            Level = "Error",
            Target = warehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });

        // DEBUG: Detaylı hata logu
        await _logManager.LogAsync(new AppLogDto
        {
            UserId = userId,
            Action = "DEBUG - Hata Detayı",
            Message = $"Hata detayı: {ex.Message}, StackTrace: {ex.StackTrace}",
            Level = "Error",
            Target = warehouseProductId.ToString(),
            Timestamp = DateTime.Now
        });
        
        throw;
    }
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
               
                StockCode = wp.StockCode,
                LocationDisplay = wp.WarehouseLocation != null ? BuildLocationHierarchy(wp.WarehouseLocation) : null,
                Barcode = wp.Product.Barcode,
                ImageUrl = wp.Product.ImageUrl,
                Description = wp.Product.Description
            }).ToList();
        }
        private string BuildLocationHierarchy(WarehouseLocation location)
        {
            var names = new List<string>();
            var current = location;

            while (current != null)
            {
                names.Insert(0, current.Name);
                current = current.Parent;
            }

            return string.Join(" > ", names);
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
               
                StockCode = warehouseProduct.StockCode,
                LocationDisplay = warehouseProduct.WarehouseLocation != null
                    ? BuildLocationHierarchy(warehouseProduct.WarehouseLocation)
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

            var parsedLevels = LocationParser.ParseDynamic(row.LocationText);
            if (parsedLevels == null || parsedLevels.Count == 0)
            {
                var errorMsg = $"Lokasyon formatı hatalı: '{row.LocationText}'";
                errors.Add($"Satır {index}: {errorMsg}");
                await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                continue;
            }

            int? locationId = await _wareHouseLocationRepository.GetLocationIdByLevelPathAsync(warehouseId, parsedLevels);
            if (locationId == null)
            {
                var errorMsg = $"Lokasyon bulunamadı: '{row.LocationText}'";
                errors.Add($"Satır {index}: {errorMsg}");
                await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                continue;
            }
            
            var location = await _wareHouseLocationRepository.GetLocationByIdAsync(locationId.Value);
            if (location == null)
            {
                var errorMsg = $"Lokasyon bilgisi alınamadı: '{row.LocationText}'";
                errors.Add($"Satır {index}: {errorMsg}");
                await LogExcelErrorAsync(index, errorMsg, row.Barcode, batchId, fileName, userId);
                continue;
            }

                    if (product.StorageType != StorageType.Undefined &&
                        location.StorageType != StorageType.Undefined &&
                        product.StorageType != location.StorageType)
                    {
                        var errorMsg = $"Depolama tipi uyuşmazlığı. Ürün: {product.StorageType}, Lokasyon: {location.StorageType}";
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
