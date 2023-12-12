using Microsoft.EntityFrameworkCore;
using Moq;
using VVS_biblioteka.Controllers;
using VVS_biblioteka;
using VVS_biblioteka.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryTest
{
    [TestClass]
    public class UserControllerTest
    {
        private UserController userController;
        private LibDbContext _dbContext;

        private Mock<IUserController> _loanControllerMock = new();

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
        public void GetLoanedBooks_ReturnsOkWithBooks()
        {
            var userId = 1;

            var loansForUser = new List<Loan>
            {
                new Loan { Id = 1, UserId = userId, BookId = 1, Date = DateTime.Now, Price = 10.99m, Days = 14 },
                new Loan { Id = 2, UserId = userId, BookId = 2, Date = DateTime.Now.AddDays(-7), Price = 8.99m, Days = 7 }
            };

            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book1", Author = "Author1", Description = "Description1", price = 19 },
                new Book { Id = 2, Title = "Book2", Author = "Author2", Description = "Description2", price = 14 }
            };

            _dbContext.Loan.AddRange(loansForUser);
            _dbContext.Book.AddRange(books);
            _dbContext.SaveChanges();

            var userController = new UserController(_dbContext);

            var result = userController.GetLoanedBooks(userId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<Book>));

            var loans = (List<Book>)okResult.Value;
            Assert.AreEqual(2, loans.Count);
        }

        [TestMethod]
        public void GetLoanedBooks_ReturnsNotFoundForNoLoans()
        {
            var userId = 1;

            var loanController = new UserController(_dbContext);

            var result = loanController.GetLoanedBooks(userId);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = (NotFoundObjectResult)result;
            Assert.AreEqual($"No loaned books found for user with ID {userId}.", notFoundResult.Value);
        }

        [TestMethod]
        public void GetLoanedBooks_ReturnsOkWithBooks_Mocked()
        {
            var userId = 1;

            var loansForUser = new List<Loan>
            {
                new Loan { Id = 1, UserId = userId, BookId = 1, Date = DateTime.Now, Price = 10.99m, Days = 14 },
                new Loan { Id = 2, UserId = userId, BookId = 2, Date = DateTime.Now.AddDays(-7), Price = 8.99m, Days = 7 }
            };

            var loanedBooks = new List<Book>
            {
                new Book { Id = 1, Title = "Book1", Author = "Author1", Description = "Description1", price = 19 },
                new Book { Id = 2, Title = "Book2", Author = "Author2", Description = "Description2", price = 14 }
            };

            _loanControllerMock.Setup(x => x.GetLoanedBooks(userId)).Returns(new OkObjectResult(loansForUser));

            var userController = _loanControllerMock.Object;
            var result = userController.GetLoanedBooks(userId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<Loan>));

            var loans = (List<Loan>) okResult.Value;
            Assert.AreEqual(2, loans.Count);
        }

        [TestMethod]
        public void GetLoanedBooks_ReturnsNotFoundForNoLoans_Mocked()
        {
            var userId = 1;

            var loansForUser = new List<Loan>();

            _loanControllerMock.Setup(x => x.GetLoanedBooks(userId)).Returns(new NotFoundObjectResult($"No loaned books found for user with ID {userId}."));

            var result = _loanControllerMock.Object.GetLoanedBooks(userId);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

            var notFoundResult = (NotFoundObjectResult)result;
            Assert.AreEqual($"No loaned books found for user with ID {userId}.", notFoundResult.Value);
        }

        [TestMethod]
        public async Task Details_ReturnsCorrectUser()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var user1 = new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    UserType = UserType.Student,
                    Email = "john.doe@example.com",
                    PasswordHash = "hashedPassword",
                    ExpirationDate = DateTime.Now.AddDays(30)
                };

                context.User.AddRange(user1);
                context.SaveChanges();

                var controller = new UserController(context);

                var result = await controller.Details(1);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Id);

                Assert.AreEqual("John", result.FirstName);
                Assert.AreEqual("Doe", result.LastName);
                Assert.AreEqual(UserType.Student, result.UserType);
                Assert.AreEqual("john.doe@example.com", result.Email);
                Assert.AreEqual("hashedPassword", result.PasswordHash);
                Assert.AreEqual(user1.ExpirationDate, result.ExpirationDate);
            }
        }

        [TestMethod]
        public async Task CreateValidUserTest()
        {
            var validRequest = new CreateUserRequest
            {
                FirstName = "Mujo",
                LastName = "Mujić",
                Email = "mujo.mujic@gmail.com",
                Password = "ValidPassword123",
                UserType = UserType.Penzioner
            };

            var result = await userController.Create(validRequest) as OkResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task CreateUserInvalidPasswordTest()
        {
            var invalidRequest = new CreateUserRequest
            {
                FirstName = "Mujo",
                LastName = "Mujić",
                Email = "mujo.mujic@gmail.com",
                Password = "NotOk",
                UserType = UserType.Penzioner
            };

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() => userController.Create(invalidRequest));
            
            Assert.AreEqual("Password must be at least 6 characters long.",result.Message);
        }

        [TestMethod]
        public async Task CreateUserInvalidNameTest()
        {
            var invalidRequest = new CreateUserRequest
            {
                FirstName = "Mujo12",
                LastName = "Mujić 5",
                Email = "mujo.mujic@gmail.com",
                Password = "ValidPassword123",
                UserType = UserType.Penzioner
            };

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() => userController.Create(invalidRequest));

            Assert.AreEqual("First name and last name can only contain letters.", result.Message);
        }

        [TestMethod]
        public async Task CreateUserInvalidEmailTest()
        {
            var invalidRequest = new CreateUserRequest
            {
                FirstName = "Mujo",
                LastName = "Mujić",
                Email = "mujo.mujic@themail.com",
                Password = "ValidPassword123",
                UserType = UserType.Penzioner
            };

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() => userController.Create(invalidRequest));

            Assert.AreEqual("Invalid email domain. Allowed domains are gmail.com, etf.unsa.ba, yahoo.com, or outlook.com.", result.Message);
        }

        [TestMethod]
        public async Task CreateUserInvalidTypeTest()
        {
            var invalidRequest = new CreateUserRequest
            {
                FirstName = "Mujo",
                LastName = "Mujić",
                Email = "mujo.mujic@gmail.com",
                Password = "ValidPassword123",
                UserType = (UserType)100
            };

            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() => userController.Create(invalidRequest));

            Assert.AreEqual("Invalid type of user!", result.Message);
        }

    }
}
