using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class BookControllerWhiteBoxTest
    {
        private BookController bookController;
        private LibDbContext _dbContext;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            bookController = new BookController(new LibDbContext(options));
            _dbContext = new LibDbContext(options);
        }

        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Student()
        {
            User user = new User { UserType=UserType.Student };
            Loan loan = new Loan { Price=10, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(7, loan.Price); 
            Assert.AreEqual(60, loan.Days);


        }

        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Ucenik()
        {
            User user = new User { UserType=UserType.Ucenik };
            Loan loan = new Loan { Price=10, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(9, loan.Price);
            Assert.AreEqual(15, loan.Days);


        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Pezioner()
        {
            User user = new User { UserType=UserType.Penzioner };
            Loan loan = new Loan { Price=100, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(85, loan.Price);
            Assert.AreEqual(30, loan.Days);


        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Dijete()
        {
            User user = new User { UserType=UserType.Dijete };
            Loan loan = new Loan { Price=100, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(95, loan.Price);
            Assert.AreEqual(10, loan.Days);


        }
        private const int DefaultLoanDays = 5;
        [TestMethod]
        public void ApplyCategorySpecificBenefits_UnknownUserType_NoDiscountApplied()
        {
            
            User user = new User { UserType = (UserType)10 };// imamo samo 4 tipa tako da je ovo nepoznat tip
            Loan loan = new Loan { Price = 100, Days = 5 };

            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(100, loan.Price); // cijena se ne smije promijeniti
            Assert.AreEqual(DefaultLoanDays, loan.Days); // po defaultu
        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_Student()
        {

            User user = new User { UserType = UserType.Student };
            Loan loan = new Loan { Price = 100, Days = 0 };

            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreNotEqual(100, loan.Price); 
            Assert.AreNotEqual(100, loan.Days); 
        }

        [TestMethod]
        public void GetBookDetails_BookNotFound()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

            var books = new List<Book>
            {
                new Book { Id = 1, Title="Test", Author="TestAuthor", Description="Description",price = 10},
                new Book { Id = 2, Title="Test Two", Author="TestAuthorTwo",Description="Description",price=12},
                new Book { Id = 3, Title="Test Three", Author="TestAuthorThree",Description="Description",price=45},
            };
            _dbContext.Book.AddRange(books);
            _dbContext.SaveChanges();
            var result = bookController.GetBookDetails(4);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetBookDetails_ReturnsDetails()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                UserType = UserType.Student,
                PasswordHash = "null"
            };
            var loan = new Loan
            {
                 Id = 1, UserId = 1, BookId = 1, Date = DateTime.Now, Price = 12.55m, Days=21 
            };
            var books = new List<Book>
            {
                new Book { Id = 1, Title="Test", Author="TestAuthor", Description="Description",price = 10},
                new Book { Id = 2, Title="Test Two", Author="TestAuthorTwo",Description="Description",price=12}
            };
            _dbContext.Book.AddRange(books);
            _dbContext.Loan.Add(loan);
            _dbContext.User.Add(user);
            _dbContext.SaveChanges();
            var result = bookController.GetBookDetails(1);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }


        [TestCleanup]
        public void CleanUp()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
