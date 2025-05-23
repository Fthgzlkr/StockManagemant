using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagemant.Entities.Enums
{
    public enum StorageType
    {
        Undefined = 1,
        ColdStorage = 2,
        Flammable = 3,
        Fragile = 4,
        Standart = 5,
        HumidProtected=6
    }
}
