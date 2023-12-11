namespace VVS_biblioteka.Models
{
    public class CreateBookRequest
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public bool Loaned { get; set; }
        public int price { get; set; }
    }
}
