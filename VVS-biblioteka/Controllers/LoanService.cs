using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    public interface ILoanService
    {
        void LoanBook(LoanRequest request, LibDbContext context);
        void ReturnBook(GetBookBackRequest request, LibDbContext context);
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

        private decimal CalculateDiscountedFee(decimal baseFee, int discountPercentage)
        {
            return baseFee - (baseFee * discountPercentage / 100);
        }
    }
}
