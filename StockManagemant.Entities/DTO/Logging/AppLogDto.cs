

namespace StockManagemant.Entities.DTO
{
   public class AppLogDto
    {
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string? Target { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public string? FileName { get; set; }
        public string? BatchId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
