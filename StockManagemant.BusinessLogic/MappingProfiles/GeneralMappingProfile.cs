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
          

            // Receipt Mappings
            CreateMap<Receipt, ReceiptDto>().ReverseMap();
         

            // ReceiptDetail Mappings
            CreateMap<ReceiptDetail, ReceiptDetailDto>().ReverseMap();


            // Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap()
     .ForMember(dest => dest.Category, opt => opt.Ignore()); 



            //WareHouse Mappings
            CreateMap<Warehouse,WareHouseDto>().ReverseMap();
            

            //WareHouse Product Mappings
            CreateMap<WarehouseProduct,WarehouseProductDto>().ReverseMap();

            //Warehouse location Mappings 
            CreateMap<WarehouseLocation ,WarehouseLocationDto>().ReverseMap();

            // AppUser Mappings
            CreateMap<AppUser, AppUserDto>().ReverseMap();
            CreateMap<AppUserCreateDto, AppUser>().ReverseMap();
            
        }
    }

}
