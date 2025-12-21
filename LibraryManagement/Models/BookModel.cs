namespace LibraryManagement.Models
{
    public class BookModel
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = "";
        public string Author { get; set; } = "";
        public string Publisher { get; set; } = "";
        public int Quantity { get; set; } = 1;
    }
}