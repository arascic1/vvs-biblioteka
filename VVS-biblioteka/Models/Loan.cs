namespace VVS_biblioteka.Models
{
    public class Loan
    {
        public int Id {  get; set; }
        public int BookId { get; set; }
        public int UserId {  get; set; }
        public DateTime Date { get; set; }

        public decimal Price { get; set; }

        public int Days { get; set; }
    }
}
