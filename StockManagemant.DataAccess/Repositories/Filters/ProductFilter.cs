using StockManagemant.Entities.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace StockManagement.DataAccess.Filters
{
    public class ProductFilter
    {
        public string Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; } // Eğer kategoriye göre filtreleme istersek
        public bool IncludeDeleted { get; set; } = false; // Silinmiş ürünleri dahil etme

        public Expression<Func<Product, bool>> GetFilterExpression()
        {
            return product =>
                (!IncludeDeleted ? !product.IsDeleted : true) &&
                (string.IsNullOrEmpty(Search) || product.Name.Contains(Search)) &&
                (!MinPrice.HasValue || product.Price >= MinPrice.Value) &&
                (!MaxPrice.HasValue || product.Price <= MaxPrice.Value) &&
                (!CategoryId.HasValue || product.CategoryId == CategoryId.Value);
        }
    }
}
