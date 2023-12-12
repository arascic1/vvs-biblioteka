using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    /// <summary>
    /// Controller for managing book-related operations.
    /// </summary>
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly LibDbContext _context;
        private readonly ILoanService _loanService;

        /// <summary>
        /// Initializes a new instance of the BookController class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="loanService">The loan service.</param>
        public BookController(LibDbContext context, ILoanService loanService)
        {
            _context = context;
            _loanService = loanService;
        }

        /// <summary>
        /// Adds a new book to the library.
        /// </summary>
        /// <param name="book">The book to add.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddBook(Book book)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        /// <summary>
        /// Gets details for a specific book.
        /// </summary>
        /// <param name="bookId">The ID of the book.</param>
        /// <returns>An action result containing book details.</returns>
        [HttpGet("{bookId}")]
        public IActionResult GetBookDetails(int bookId)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
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

        /// <summary>
        /// Loans a book to a user.
        /// </summary>
        /// <param name="request">The loan request.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost("loan")]
        public async Task<IActionResult> LoanBook(LoanRequest request)
        {
            try
            {
                _loanService.LoanBook(request, _context);
                return Ok("You loaned a book successfully!");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
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

        /// <summary>
        /// Returns a book previously loaned by a user.
        /// </summary>
        /// <param name="request">The request to get the book back.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpDelete("return")]
        public async Task<IActionResult> GetBookBack(GetBookBackRequest request)
        {
            try
            {
                _loanService.ReturnBook(request, _context);
                return Ok("You got the book back!");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}