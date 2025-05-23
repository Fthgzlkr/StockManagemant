using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using StockManagemant.Entities.Enums;
using AutoMapper;
using StockManagemant.DataAccess.Repositories.Interfaces;


namespace StockManagemant.Business.Managers
{
    public class ProductManager : IProductManager
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogManager _logManager;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public ProductManager(IProductRepository productRepository,IMapper mapper, ICategoryRepository categoryRepository,ILogManager logManager)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _logManager=logManager;
        }

        //  Toplam ürün sayısını getir (Silinmemiş olanlar)
        public async Task<int> GetTotalProductCountAsync(ProductFilter filter)
        {
        
            return await _productRepository.GetTotalCountAsync(filter);
        }

        public async Task<List<ProductDto>> GetPagedProductAsync(int page, int pageSize, ProductFilter filter)
        {
            var products = await _productRepository.GetPagedProductsAsync(filter, page, pageSize); //  Sayfalama işlemi repoda yapılıyor
            return _mapper.Map<List<ProductDto>>(products); //  DTO dönüşümü burada yapılıyor
        }




        public async Task<int> AddProductAsync(ProductDto productDto)
        {
            var optionalWarnings = new List<string>();

            if (string.IsNullOrWhiteSpace(productDto.Barcode))
                optionalWarnings.Add("Barcode");
            if (string.IsNullOrWhiteSpace(productDto.ImageUrl))
                optionalWarnings.Add("ImageUrl");
            if (string.IsNullOrWhiteSpace(productDto.Description))
                optionalWarnings.Add("Description");

            if (optionalWarnings.Any())
            {
                await _logManager.LogAsync(new AppLogDto
                {
                    UserId = 0,
                    Action = " Uygulamadan Ürün Oluşturma",
                    Level = "Warning",
                    FileName = "Dosyasız İşlem",
                    Message = $" Boş olan alan yada alanlar: {string.Join(", ", optionalWarnings)}",
                    Target = productDto.Name ?? productDto.Barcode ?? "Bilinmeyen",
                    Timestamp = DateTime.Now
                });
            }

            var product = _mapper.Map<Product>(productDto);

            product.Category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (product.Category == null)
            {
                throw new Exception($"Kategori ID {productDto.CategoryId} ile kategori bulunamadı.");
            }

            await _productRepository.AddAsync(product);
            return product.Id;
        }



        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(productDto.Id ?? 0);
            if (existingProduct == null) throw new Exception("Ürün bulunamadı.");

            _mapper.Map(productDto, existingProduct);

            existingProduct.Category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (existingProduct.Category == null) throw new Exception("Geçersiz kategori.");

            await _productRepository.UpdateAsync(existingProduct);
        }


       
        public async Task DeleteProductAsync(int productId)
        {
            await _productRepository.DeleteAsync(productId);
        }

        //  Soft Delete olan ürünü geri getirme (Restore)
        public async Task RestoreProductAsync(int productId)
        {
            await _productRepository.RestoreAsync(productId);
        }

        //  ID’ye göre ürün bulma (Sadece aktif ürünleri getirir)
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

         public async Task<ProductDto> GetProductByBarcode(string barcode)
        {
            var product = await _productRepository.GetProductByBarcodeAsync(barcode);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        // ID’ye göre ürün bulma (Silinmiş ürünler dahil)
        public async Task<ProductDto> GetProductByIdWithDeletedAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDeletedAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> IsProductInWarehouseAsync(int productId, int warehouseId)
        {
            return await _productRepository.IsProductInWarehouseAsync(productId, warehouseId);
        }

public async Task<(int insertedCount, List<string> errors)> AddProductsFromExcelAsync(
    List<RawProductModel> rawProducts, int userId, string fileName)
{
    var errors = new List<string>();
    var validProducts = new List<Product>();
    int rowIndex = 2;
    var batchId = Guid.NewGuid().ToString();

    foreach (var raw in rawProducts)
    {
        var errorPrefix = $"Satır {rowIndex}: ";
        var rowErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(raw.Name))
            rowErrors.Add("Ürün adı boş olamaz.");

        if (string.IsNullOrWhiteSpace(raw.CategoryName))
            rowErrors.Add("Kategori adı boş olamaz.");

        if (string.IsNullOrWhiteSpace(raw.CurrencyText))
            rowErrors.Add("Para birimi boş olamaz.");

        if (!Enum.TryParse(raw.CurrencyText?.Trim(), true, out CurrencyType currency))
        {
            rowErrors.Add($"Geçersiz para birimi: {raw.CurrencyText}");
        }

        StorageType storageType = StorageType.Undefined;
        if (!string.IsNullOrWhiteSpace(raw.StorageTypeText))
        {
            if (!Enum.TryParse(raw.StorageTypeText.Trim(), true, out storageType))
                rowErrors.Add($"Geçersiz depolama türü: {raw.StorageTypeText}");
        }

        decimal? price = null;
        if (!string.IsNullOrWhiteSpace(raw.Price))
        {
            if (decimal.TryParse(raw.Price, out var parsedPrice))
                price = parsedPrice;
            else
                rowErrors.Add($"Geçersiz fiyat: {raw.Price}");
        }

        var categoryList = await _categoryRepository.FindAsync(c =>
            c.Name.ToLower() == raw.CategoryName.Trim().ToLower() && !c.IsDeleted);

        var matchedCategory = categoryList.FirstOrDefault();
        if (matchedCategory == null)
        {
            rowErrors.Add($"Kategori bulunamadı: {raw.CategoryName}");
        }

        if (rowErrors.Any())
        {
            string combinedError = errorPrefix + string.Join(" | ", rowErrors);
            errors.Add(combinedError);

            await _logManager.LogAsync(new AppLogDto
            {
                UserId = userId,
                Action = "Excel Ürün Yükleme",
                Message = combinedError,
                Level = "Error",
                FileName = fileName,
                BatchId = batchId,
                Target = raw.Barcode ?? raw.Name ?? "Bilinmeyen",
                Timestamp = DateTime.Now
            });
        }
        else
        {
            validProducts.Add(new Product
            {
                Name = raw.Name.Trim(),
                CategoryId = matchedCategory.Id,
                Currency = currency,
                Price = price,
                Barcode = raw.Barcode?.Trim(),
                ImageUrl = raw.ImageUrl?.Trim(),
                Description = raw.Description?.Trim(),
                StorageType = storageType,
                IsDeleted = false
            });
        }

        rowIndex++;
    }

    if (validProducts.Any())
    {
        await _productRepository.BulkInsertAsync(validProducts);
    }

    return (validProducts.Count, errors);
}

    }
}
