using Microsoft.AspNetCore.Mvc.Rendering;
using StockManagemant.DataAccess.Filters;
using StockManagemant.Entities.Models;

namespace StockManagemant.Web.WievModels
{
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public ProductFilter Filter { get; set; } = new ProductFilter();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public string SortColumn { get; set; } = "id";
        public string SortOrder { get; set; } = "asc";
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
