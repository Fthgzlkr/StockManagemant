namespace StockManagemant.DataAccess.LoggingModels
{
    public class AppLogEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Nullable: anonim işlemler için
        public string Action { get; set; } = null!;
        public string? Target { get; set; }
        public string Message { get; set; } = null!;
        public string Level { get; set; } = "Info"; // Info, Warning, Error
        public string? FileName { get; set; }
        public string? BatchId { get; set; }
        public DateTime Timestamp { get; set; } 
    }
}