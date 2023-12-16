using VVS_biblioteka.Controllers;
using VVS_biblioteka;
using VVS_biblioteka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace LibraryTest
{
    [TestClass]
    public class UserControllerWhiteBoxTest
    {
        private UserController userController;
        private LibDbContext _dbContext;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            userController = new UserController(new LibDbContext(options));
            _dbContext = new LibDbContext(options);
        }

        [TestMethod]
        public void GetLoanedBooksTest1()
        {
            var userId = 1;
            var loans = new List<Loan>
            {
                new Loan { Id = 1, UserId = userId, BookId = 1, Date = DateTime.Now, Price = 12.55m, Days=21 },
                new Loan { Id = 2, UserId = userId, BookId = 2, Date = DateTime.Now, Price = 23.80m, Days=31 },
                new Loan { Id = 3, UserId = userId, BookId = 3, Date = DateTime.Now, Price = 7.45m, Days=7},
            };
            var books = new List<Book>
            {
                new Book { Id = 1, Title="Test", Author="TestAuthor", Description="Description",price = 10},
                new Book { Id = 2, Title="Test Two", Author="TestAuthorTwo",Description="Description",price=12},
                new Book { Id = 3, Title="Test Three", Author="TestAuthorThree",Description="Description",price=45},
            };
            _dbContext.Loan.AddRange(loans);
            _dbContext.Book.AddRange(books);
            _dbContext.SaveChanges();
            var result = userController.GetLoanedBooks(userId);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetLoanedBooksTest2()
        {
            var userId = 2;
            var loans = new List<Loan>
            {
                new Loan { Id = 1, UserId = 1, BookId = 1, Date = DateTime.Now, Price = 12.55m, Days=21 },
            };
            var books = new List<Book>
            {
                new Book { Id = 1, Title="Test", Author="TestAuthor", Description="Description",price = 10}
            };
            _dbContext.Loan.AddRange(loans);
            _dbContext.Book.AddRange(books);
            _dbContext.SaveChanges();
            var result = userController.GetLoanedBooks(userId);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestCleanup]
        public void CleanUp()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
