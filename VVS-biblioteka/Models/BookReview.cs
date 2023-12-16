namespace VVS_biblioteka.Models
{
    public class BookReview
    {
        public int BookReviewId { get; set; }
        public int BookId { get; set; }
        public int Grade { get; set; }
        public string Message { get; set; }
    }
}
