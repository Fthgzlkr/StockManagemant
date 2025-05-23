using StockManagemant.Entities.Models;
using StockManagemant.Entities.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace StockManagemant.DataAccess.Filters
{
    public class ProductFilter
    {
        public string Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Barcode { get; set; }
        public int? CategoryId { get; set; } // Eğer kategoriye göre filtreleme istersek
        public bool IncludeDeleted { get; set; } = false; // Silinmiş ürünleri dahil etme
        public StorageType? StorageType { get; set; }

        public Expression<Func<Product, bool>> GetFilterExpression()
        {

            //if (string.IsNullOrEmpty(Search))
            //{
            //    product = product.Name.Contains(Search);
            //}
            return product =>
                (!IncludeDeleted ? !product.IsDeleted : true) &&
                (string.IsNullOrEmpty(Search) || product.Name.Contains(Search)) &&
                (string.IsNullOrEmpty(Barcode) || product.Barcode.Contains(Barcode)) &&
                (!MinPrice.HasValue || product.Price >= MinPrice.Value) &&
                (!MaxPrice.HasValue || product.Price <= MaxPrice.Value) &&
                (!CategoryId.HasValue || product.CategoryId == CategoryId.Value)&&
                (!StorageType.HasValue || product.StorageType == StorageType.Value);
        }
    }
}
