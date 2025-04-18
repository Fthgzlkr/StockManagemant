using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
  public class AppUserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public int? AssignedWarehouseId { get; set; }
}

public class AppUserLoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AppUserCreateDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public int? AssignedWarehouseId { get; set; }
}
}
