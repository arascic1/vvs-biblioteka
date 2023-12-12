using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    /// <summary>
    /// Interface for handling book loan operations.
    /// </summary>
    public interface ILoanService
    {
        /// <summary>
        /// Loans a book to a user.
        /// </summary>
        /// <param name="request">The loan request.</param>
        /// <param name="context">The database context.</param>
        void LoanBook(LoanRequest request, LibDbContext context);

        /// <summary>
        /// Returns a book previously loaned by a user.
        /// </summary>
        /// <param name="request">The request to get the book back.</param>
        /// <param name="context">The database context.</param>
        void ReturnBook(GetBookBackRequest request, LibDbContext context);

        /// <summary>
        /// Gets loan details for a specific book.
        /// </summary>
        /// <param name="bookId">The ID of the book.</param>
        /// <param name="context">The database context.</param>
        /// <returns>An object containing loan and user details.</returns>
        object GetBookLoanDetails(int bookId, LibDbContext context);
    }

    public class LoanService : ILoanService
    {
        public void LoanBook(LoanRequest request, LibDbContext context)
        {
            Book book = context.Book.FirstOrDefault(b => b.Id == request.BookId);

            var isBookLoaned = context.Loan.Any(l => l.BookId == book.Id);

            if (isBookLoaned)
            {
                throw new ArgumentException("Book is already loaned!");
            }

            Loan loan = context.Loan.FirstOrDefault(l => l.UserId == request.UserId);

            User user = context.User.FirstOrDefault(u => u.Id == request.UserId);

            if (loan != null)
            {
                throw new ArgumentException("You already loaned a book!");
            }

            Loan newLoan = new()
            {
                BookId = request.BookId,
                UserId = request.UserId,
                Date = DateTime.Today,
                Price = book.price,
            };

            ApplyCategorySpecificBenefits(user, newLoan);

            context.Loan.Add(newLoan);
            context.SaveChanges();
        }

        public void ReturnBook(GetBookBackRequest request, LibDbContext context)
        {
            Loan loan = context.Loan.FirstOrDefault(l => l.BookId == request.BookId);

            if (loan == null)
            {
                throw new ArgumentException("Book with that id is not loaned!");
            }

            context.Loan.Remove(loan);
            context.SaveChanges();
        }

        public object GetBookLoanDetails(int bookId, LibDbContext context)
        {
            var loan = context.Loan.FirstOrDefault(l => l.BookId == bookId);
            var user = loan != null ? context.User.FirstOrDefault(u => u.Id == loan.UserId) : null;

            return new
            {
                Loan = loan,
                User = user
            };
        }

        /// <summary>
        /// Applies category-specific benefits to the loan based on user type.
        /// </summary>
        /// <param name="user">The user for whom the book is loaned.</param>
        /// <param name="loan">The loan object to which benefits are applied.</param>
        private void ApplyCategorySpecificBenefits(User user, Loan loan)
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

        /// <summary>
        /// Calculates the discounted fee based on a given percentage.
        /// </summary>
        /// <param name="baseFee">The base fee to apply the discount to.</param>
        /// <param name="discountPercentage">The percentage by which to discount the fee.</param>
        /// <returns>The discounted fee.</returns>
        private decimal CalculateDiscountedFee(decimal baseFee, int discountPercentage)
        {
            return baseFee - (baseFee * discountPercentage / 100);
        }
    }
}
