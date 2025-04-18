using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagemant.Entities.Models
{
  public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } 
    public int? AssignedWarehouseId { get; set; } 
     public Warehouse? AssignedWarehouse { get; set; }
    public bool IsDeleted { get; set; } = false; // Soft Delete için
}
}
