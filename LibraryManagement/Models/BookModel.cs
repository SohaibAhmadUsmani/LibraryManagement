namespace LibraryManagement.Models
{
    public class BookModel
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
