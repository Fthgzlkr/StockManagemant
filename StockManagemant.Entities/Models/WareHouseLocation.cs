using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagemant.Entities.Models
{
    public class WarehouseLocation
    {
    public int Id { get; set; }

    public int WarehouseId { get; set; }  // Hangi Depo
    public string Corridor { get; set; } = null!;
    public string? Shelf { get; set; }
    public string? Bin { get; set; }
    public bool IsDeleted { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    }


}