using System.ComponentModel.DataAnnotations;
using StockManagemant.Entities.Enums;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
  public class WarehouseLocationDto
  {
    public int Id { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public string Name { get; set; }

    public int? ParentId { get; set; }

    [Required]
    public LocationLevel Level { get; set; }
    public StorageType StorageType { get; set; } = StorageType.Undefined;
}
}
