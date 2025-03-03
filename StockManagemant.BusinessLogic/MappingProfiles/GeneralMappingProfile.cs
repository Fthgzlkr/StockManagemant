using AutoMapper;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;


namespace StockManagemant.Business.MappingProfiles
{
    public class GeneralMappingProfile : Profile
    {
        public GeneralMappingProfile()
        {
            // Category Mappings
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, UpdateCategoryDto>().ReverseMap();

            // Receipt Mappings
            CreateMap<Receipt, ReceiptDto>().ReverseMap();
            CreateMap<Receipt, CreateReceiptDto>().ReverseMap();
            CreateMap<Receipt, UpdateReceiptDto>().ReverseMap();

            // ReceiptDetail Mappings
            CreateMap<ReceiptDetail, ReceiptDetailDto>().ReverseMap();
            CreateMap<ReceiptDetail, CreateReceiptDetailDto>().ReverseMap();
            CreateMap<ReceiptDetail, UpdateReceiptDetailDto>().ReverseMap();

            // Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap();

            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();

            //WareHouse Mappings
            CreateMap<Warehouse,WareHouseDto>().ReverseMap();
            CreateMap<Warehouse,CreateWarehouseDto>().ReverseMap();
            CreateMap<Product, UpdateWarehouseDto>().ReverseMap();

            //WareHouse Product Mappings
            CreateMap<WarehouseProduct,WarehouseProductDto>().ReverseMap();
            CreateMap<WarehouseProduct,UpdateWarehouseProductStockDto>().ReverseMap();
            CreateMap< WarehouseProduct, AddExistingProductToWarehouseDto >().ReverseMap();
        }
    }

}
