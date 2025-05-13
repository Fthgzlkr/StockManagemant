using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagemant.Entities.Enums
{
      public enum ReceiptType 
    {
        Entry = 1,
        Exit = 2
    }

      public enum ReceiptSourceType
    {
            // varsayılan
        Warehouse = 1,    // başka bir depo
        Customer = 2,     // müşteri
         None = 3     // tedarikçi (varsa)
    }
}
