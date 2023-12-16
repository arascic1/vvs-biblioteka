using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    [ApiController]
    [Route("bookController")]
    public class BookController : ControllerBase
    {
        private readonly LibDbContext _context;

        public BookController(LibDbContext context) 
        {
            _context = context;
        }

        [HttpPost]
        [Route("addBook")]
        public async Task<IActionResult> AddBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Book.Any(b => b.Id == book.Id))
            {
                return BadRequest("Book with the same Id already exists.");
            }

            _context.Book.Add(book);
            await _context.SaveChangesAsync();

            return Ok("Book added successfully!");
        }

        [HttpGet]
        [Route("getBookDetails/{bookId}")]
        public IActionResult GetBookDetails(int bookId)
        {
            var book = _context.Book.FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                return NotFound($"Book with ID {bookId} not found.");
            }

            var loan = _context.Loan.FirstOrDefault(l => l.BookId == bookId);

            var user = loan != null ? _context.User.FirstOrDefault(u => u.Id == loan.UserId) : null;

            var bookDetails = new
            {
                Book = book,
                Loan = loan,
                User = user
            };

            return Ok(bookDetails);
        }

        [HttpDelete]
        [Route("deleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var bookToDelete = await _context.Book.FindAsync(id);

            if (bookToDelete == null)
            {
                return NotFound($"Book with ID {id} not found.");
            }

            _context.Book.Remove(bookToDelete);
            await _context.SaveChangesAsync();

            return Ok($"Book with ID {id} deleted successfully.");
        }
        [NonAction]
        public void ApplyCategorySpecificBenefits(User user, Loan loan)
        {
            switch (user.UserType)
            {
                case UserType.Student:
                    loan.Price = CalculateDiscountedFee(loan.Price, 30);
                    loan.Days = 60;
                    break;
                case UserType.Ucenik:
                    loan.Price = CalculateDiscountedFee(loan.Price, 10);
                    loan.Days = 15;
                    break;
                case UserType.Penzioner:
                    loan.Price = CalculateDiscountedFee(loan.Price, 15);
                    loan.Days = 30;
                    break;
                case UserType.Dijete:
                    loan.Price = CalculateDiscountedFee(loan.Price, 5);
                    loan.Days = 10;
                    break;
            }
        }

        private decimal CalculateDiscountedFee(decimal baseFee, int discountPercentage)
        {
            return baseFee - (baseFee * discountPercentage / 100);
        }

        [HttpPost]
        public async Task<LoanBookResult> LoanBook(LoanRequest request)
        {
            Book book = _context.Book.FirstOrDefault(b => b.Id == request.BookId);

            var isBookLoaned = _context.Loan.Any(l => l.BookId == book.Id);

            if (isBookLoaned)
            {
                return new LoanBookResult { Success = false, Message = "Book is already loaned!" };
            }

            Loan loan = _context.Loan.FirstOrDefault(l => l.UserId == request.UserId);

            User user = _context.User.FirstOrDefault(u => u.Id == request.UserId);

            if (loan != null)
            {
                return new LoanBookResult { Success = false, Message = "You already loaned book!" };
            }

            Loan l = new()
            {
                BookId = request.BookId,
                UserId = request.UserId,
                Date = DateTime.Today,
                Price = book.price,
            };

            _context.Loan.Add(l);
            await _context.SaveChangesAsync();
            return new LoanBookResult { Success = true, Message = "Ok" };
        }

        [HttpGet]
        [Route("searchBooks")]
        public IActionResult SearchBooks(string? title, string? author)
        {
            var books = _context.Book
                .Where(b =>
                    (string.IsNullOrEmpty(title) || b.Title.Contains(title)) &&
                    (string.IsNullOrEmpty(author) || b.Author.Contains(author)))
                .ToList();

            return Ok(books);
        }

        [HttpDelete]
        [Route("/{getBookBack}")]
        public async Task<GetBookBackResult> GetBookBack(GetBookBackRequest request)
        {
            Loan l = _context.Loan.FirstOrDefault(l => l.BookId == request.BookId);
            if (l == null)
            {
                return new GetBookBackResult { Success = false, Message = "Book with that id is not loaned!" };

            }
            _context.Loan.Remove(l);

            await _context.SaveChangesAsync();
            return new GetBookBackResult { Success = true, Message = "You got book back!" };

        }

        [HttpPost]
        [Route("/addBookReview")]
        public async Task<AddBookReviewResponse> AddBookReview(BookReview review)
        {
            Book book = _context.Book.FirstOrDefault(b => b.Id == review.BookId);
            if (book == null)
            {
                return new AddBookReviewResponse
                {
                    Success = false,
                    Message = "Book with that Id is not found!"
                };
            }
            if (review.Grade < 1)
            {
                return new AddBookReviewResponse
                {
                    Success = false,
                    Message = "Grade cannot be less than 1!"
                };

            }

            if (review.Grade > 5)
            {
                return new AddBookReviewResponse
                {
                    Success = false,
                    Message = "Grade cannot be greater than 5!"
                };
            }

            _context.BookReview.Add(review);
            await _context.SaveChangesAsync();
            return new AddBookReviewResponse
            {
                Success = true,
                Message = "Ok!"
            };
        }

        [HttpGet]
        [Route("/getAverageGrade")]
        public async Task<GetAverageGradeResponse> GetAverageGrade(int id)
        {
            var reviewList = _context.BookReview.Where(r => r.BookId == id).ToList();
            if (reviewList.Count < 1 || reviewList == null)
            {
                return new GetAverageGradeResponse
                {
                    Success = false,
                    Message = "Book with that Id is not found!",
                    Value = -1
                };
            }
            var value = reviewList.Average(r => r.Grade);

            return new GetAverageGradeResponse
            {
                Success = true,
                Message = "Ok!",
                Value = value
            };
        }

        [HttpDelete]
        [Route("/deleteReview")]
        public async Task<DeleteReviewResponse> DeleteReview(int id)
        {
            BookReview review = await _context.BookReview.FirstOrDefaultAsync(r => r.BookReviewId == id);

            if (review == null)
            {
                return new DeleteReviewResponse
                {
                    Success = false,
                    Message = "Review with that Id is not found!"
                };
            }

            _context.BookReview.Remove(review);
            await _context.SaveChangesAsync();

            return new DeleteReviewResponse
            {
                Success = true,
                Message = "Ok!"
            };
        }
    }
}
