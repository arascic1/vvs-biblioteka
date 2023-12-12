using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly LibDbContext _context;
        private readonly ILoanService _loanService;

        public BookController(LibDbContext context, ILoanService loanService)
        {
            _context = context;
            _loanService = loanService;
        }

        [HttpPost("add")]
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

        [HttpGet("{bookId}")]
        public IActionResult GetBookDetails(int bookId)
        {
            var book = _context.Book.FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                return NotFound($"Book with ID {bookId} not found.");
            }

            var bookDetails = new
            {
                Book = book,
                LoanDetails = _loanService.GetBookLoanDetails(bookId, _context)
            };

            return Ok(bookDetails);
        }

        [HttpDelete("{id}")]
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

        [HttpPost("loan")]
        public async Task<IActionResult> LoanBook(LoanRequest request)
        {
            _loanService.LoanBook(request, _context);

            return Ok("You loaned a book successfully!");
        }

        [HttpGet("search")]
        public IActionResult SearchBooks(string? title, string? author)
        {
            var books = _context.Book
                .Where(b =>
                    (string.IsNullOrEmpty(title) || b.Title.Contains(title)) &&
                    (string.IsNullOrEmpty(author) || b.Author.Contains(author)))
                .ToList();

            return Ok(books);
        }

        [HttpDelete("return")]
        public async Task<IActionResult> GetBookBack(GetBookBackRequest request)
        {
            _loanService.ReturnBook(request, _context);

            return Ok("You got the book back!");
        }
    }
}