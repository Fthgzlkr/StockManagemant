using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockManagemant.Entities.Enums;
namespace StockManagemant.Entities.Models
{
  public class WarehouseLocation
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    public int? ParentId { get; set; }
    public WarehouseLocation? Parent { get; set; }
    public ICollection<WarehouseLocation> Children { get; set; } = new List<WarehouseLocation>();
    public ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

    [Required]
    public LocationLevel Level { get; set; } // enum: Corridor = 1, Shelf = 2, Bin = 3
    public StorageType StorageType { get; set; } = StorageType.Undefined;

    [Required]
    public bool IsDeleted { get; set; } = false;
}

}