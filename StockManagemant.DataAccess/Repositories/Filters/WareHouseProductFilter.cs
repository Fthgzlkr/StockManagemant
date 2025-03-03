using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using StockManagemant.Entities.Models;

namespace StockManagemant.DataAccess.Repositories.Filters
{
    public class WarehouseProductFilter
    {
        public string Search { get; set; }
        public int? WarehouseId { get; set; } // Depoya göre filtreleme
        public int? ProductId { get; set; } // Ürüne göre filtreleme
        public int? CategoryId { get; set; } // Kategoriye göre filtreleme
        public decimal? MinPrice { get; set; } // Minimum fiyat filtresi
        public decimal? MaxPrice { get; set; } // Maksimum fiyat filtresi
        public bool IncludeDeleted { get; set; } = false; // Silinmişleri dahil etme

        public Expression<Func<WarehouseProduct, bool>> GetFilterExpression()
        {
            return wp =>
                (!IncludeDeleted ? !wp.IsDeleted : true) &&
                (string.IsNullOrEmpty(Search) || wp.Product.Name.Contains(Search)) &&
                (!WarehouseId.HasValue || wp.WarehouseId == WarehouseId.Value) &&
                (!ProductId.HasValue || wp.ProductId == ProductId.Value) &&
                (!CategoryId.HasValue || wp.Product.CategoryId == CategoryId.Value) &&
                (!MinPrice.HasValue || wp.Product.Price >= MinPrice.Value) &&
                (!MaxPrice.HasValue || wp.Product.Price <= MaxPrice.Value);
        }
    }
}
