﻿using Microsoft.AspNetCore.Mvc;
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
        [Route("/{loanBook}")]
        public async Task<IActionResult> LoanBook(LoanRequest request)
        {
            Book book = _context.Book.FirstOrDefault(b => b.Id == request.BookId);

            var isBookLoaned = _context.Loan.Any(l => l.BookId == book.Id);

            if (isBookLoaned)
            {
                throw new ArgumentException("Book is already loaned!");
            }

            Loan loan = _context.Loan.FirstOrDefault(l => l.UserId == request.UserId);

            User user = _context.User.FirstOrDefault(u => u.Id == request.UserId);

            if (loan != null)
            {
                throw new ArgumentException("You already loaned book!");
            }

            Loan l = new()
            {
                BookId = request.BookId,
                UserId = request.UserId,
                Date = DateTime.Today,
                Price = book.price,
            };

            ApplyCategorySpecificBenefits(user, l);

            _context.Loan.Add(l);
            await _context.SaveChangesAsync();
            return Ok("You loaned a book successfully!");
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
        public async Task<IActionResult> GetBookBack(GetBookBackRequest request)
        {
            Loan l=_context.Loan.FirstOrDefault(l => l.BookId == request.BookId);
            if (l == null)
            {
                throw new ArgumentException("Book with that id is not loaned!");
            }
            _context.Loan.Remove(l);

            await _context.SaveChangesAsync();
            return Ok("You got book back!");
        }
        
    }
}
