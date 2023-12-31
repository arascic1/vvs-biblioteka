﻿using Microsoft.EntityFrameworkCore;
using Moq;
using VVS_biblioteka.Controllers;
using VVS_biblioteka;
using VVS_biblioteka.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Xml.Linq;
using Newtonsoft.Json;

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
        public void GetLoanedBooks_ReturnsNotFoundForNonExistentUserId()
        {
            var nonExistentUserId = 3;

            var nonExistentResult = userController.GetLoanedBooks(nonExistentUserId);

            Assert.IsInstanceOfType(nonExistentResult, typeof(NotFoundObjectResult));

            var notFoundResult = (NotFoundObjectResult)nonExistentResult;
            Assert.AreEqual($"No loaned books found for user with ID {nonExistentUserId}.", notFoundResult.Value);
        }

        [TestMethod]
        public void GetLoanedBooks_ReturnsOkWithValidUserId()
        {
            var validUserId = 1;

            var validLoansForUser = new List<Loan>
            {
                new Loan { Id = 1, UserId = validUserId, BookId = 1, Date = DateTime.Now, Price = 10.99m, Days = 14 },
                new Loan { Id = 2, UserId = validUserId, BookId = 2, Date = DateTime.Now.AddDays(-7), Price = 8.99m, Days = 7 }
            };

            var validBooks = new List<Book>
            {
                new Book { Id = 1, Title = "Book1", Author = "Author1", Description = "Description1", price = 19 },
                new Book { Id = 2, Title = "Book2", Author = "Author2", Description = "Description2", price = 14 }
            };

            _dbContext.Loan.AddRange(validLoansForUser);
            _dbContext.Book.AddRange(validBooks);
            _dbContext.SaveChanges();

            var validResult = userController.GetLoanedBooks(validUserId);

            Assert.IsInstanceOfType(validResult, typeof(OkObjectResult));

            var okResult = (OkObjectResult)validResult;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<Book>));

            var loans = (List<Book>)okResult.Value;
            Assert.AreEqual(2, loans.Count);
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
        public async Task CreateValidUserWithJSONTest()
        {
            var jsonFilePath = "testData.json";
            var jsonData = File.ReadAllText(jsonFilePath);
            var testDataList = JsonConvert.DeserializeObject<List<TestData>>(jsonData);

            foreach (var testData in testDataList)
            {
                var validRequest = new CreateUserRequest
                {
                    FirstName = testData.FirstName,
                    LastName = testData.LastName,
                    Email = testData.Email,
                    Password = testData.Password,
                    UserType = Enum.Parse<UserType>(testData.UserType)
                };

                var result = await userController.Create(validRequest) as OkResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);
            }
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
        public async Task CreateValidUserWithXMLTest()
        {
            var xmlFilePath = "testdata.xml";
            var xmlData = XDocument.Load(xmlFilePath);
            var validRequest = new CreateUserRequest
            {
                FirstName = xmlData.Element("userRequest").Element("FirstName").Value,
                LastName = xmlData.Element("userRequest").Element("LastName").Value,
                Email = xmlData.Element("userRequest").Element("Email").Value,
                Password = xmlData.Element("userRequest").Element("Password").Value,
                UserType = Enum.Parse<UserType>(xmlData.Element("userRequest").Element("UserType").Value)
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

        [TestMethod]
        public async Task RenewMembership_ThrowsArgumentException()
        {
            var req = new RenewalRequest
            {
                UserId = 1,
                Months = 0
            };
            var result = await Assert.ThrowsExceptionAsync<ArgumentException>(() => userController.RenewMembership(req));
            Assert.AreEqual("Number of months must be greater than 0.", result.Message);
        }

        [TestMethod]
        public async Task RenewMembership_ThrowsHttpRequestException()
        {
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
            _dbContext.User.Add(user1);
            _dbContext.SaveChanges();
            var req = new RenewalRequest
            {
                UserId = 2,
                Months = 2
            };
            var result = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => userController.RenewMembership(req));
            Assert.AreEqual("User not found.", result.Message);
        }

        [TestMethod]
        public async Task RenewMembership_ValidRenewal()
        {
            var user1 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                UserType = UserType.Student,
                Email = "john.doe@example.com",
                PasswordHash = "hashedPassword",
                ExpirationDate = DateTime.Now.AddMonths(12)
            };
            _dbContext.User.Add(user1);
            _dbContext.SaveChanges();
            var req = new RenewalRequest
            {
                UserId = 1,
                Months = 2
            };
            var result = await userController.RenewMembership(req) as OkResult;
            var user = await userController.Details(1);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(DateTime.Now.AddMonths(14).Date, user.ExpirationDate.Date);
        }

        [TestMethod]
        public async Task InvalidLoginTest()
        {
            var invalidLoginRequest = new LoginRequest
            {
                Email = "test.user@etf.unsa.ba",
                Password = "notvalidpassword"
            };

            var validRequest = new CreateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@etf.unsa.ba",
                Password = "validpassword",
                UserType = UserType.Student,
            };

            var create = await userController.Create(validRequest) as OkResult;
            Assert.IsNotNull(create);

            var result = await Assert.ThrowsExceptionAsync<SecurityTokenException>(() => userController.Login(invalidLoginRequest));

            Assert.AreEqual("Invalid email or password", result.Message); ;
        }

        [TestMethod]
        public async Task SearchUsersTest()
        {
            var keyword = "John";
            var user1 = new CreateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "test.user@etf.unsa.ba",
                Password = "validpassword",
                UserType = UserType.Student
            };
            var user2 = new CreateUserRequest
            {
                FirstName = "Johnny",
                LastName = "Doe",
                Email = "test.user2@etf.unsa.ba",
                Password = "validpassword",
                UserType = UserType.Student,
            };

            var create1 = await userController.Create(user1) as OkResult;
            var create2 = await userController.Create(user2) as OkResult;

            var result = userController.SearchUsers(keyword) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public void SearchUsersInvalidKeywordTest()
        {
            string keyword = "";
            var result = Assert.ThrowsException<ArgumentException>(() => userController.SearchUsers(keyword));
            Assert.AreEqual("Search keyword cannot be empty.", result.Message);
        }

        [TestMethod]
        public void SearchUsers_ExceptionThrown_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDb")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.User.Add(new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    UserType = UserType.Student,
                    PasswordHash = "null"
                });

                context.SaveChanges();
            }

            var faultyContext = new LibDbContext(options)
            {
                User = new FaultyDbSet<User>()
            };

            var controller = new UserController(faultyContext);

            var result = controller.SearchUsers("testKeyword");

            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);

            dynamic errorData = objectResult.Value;
            Assert.IsNotNull(errorData);
            Assert.IsTrue(errorData.ToString().Contains("Message = Error searching users."));
        }

        [TestMethod]
        public void SortAlphanumerically_SortsUsersCorrectly()
        {
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", UserType = UserType.Student, PasswordHash = "null" },
                new User { Id = 2, FirstName = "Alice", LastName = "Smith", Email = "alice.smith@example.com", UserType = UserType.Student, PasswordHash = "null" },
                new User { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", UserType = UserType.Student, PasswordHash = "null" },
                new User { Id = 4, FirstName = "Gates", LastName = "Adams", Email = "alice.adams@example.com", UserType = UserType.Student, PasswordHash = "null" },
                new User { Id = 5, FirstName = "Alice", LastName = "Adams", Email = "another.adams@example.com", UserType = UserType.Student, PasswordHash = "null" },
            };

            var sortedUsers = UserController.SortAlphanumerically(users);

            Assert.IsNotNull(sortedUsers);
            Assert.AreEqual(5, sortedUsers.Count);

            Assert.AreEqual("Adams", sortedUsers[0].LastName);
            Assert.AreEqual("Alice", sortedUsers[0].FirstName);

            Assert.AreEqual("Adams", sortedUsers[1].LastName);
            Assert.AreEqual("Gates", sortedUsers[1].FirstName);

            Assert.AreEqual("Doe", sortedUsers[2].LastName);
            Assert.AreEqual("John", sortedUsers[2].FirstName);

            Assert.AreEqual("Johnson", sortedUsers[3].LastName);
            Assert.AreEqual("Bob", sortedUsers[3].FirstName);

            Assert.AreEqual("Smith", sortedUsers[4].LastName);
            Assert.AreEqual("Alice", sortedUsers[4].FirstName);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException), "User not found")]
        public void GetCurrentUser_UserNotFound_ThrowsHttpRequestException()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDb")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
                MockHttpSession mockSession = new MockHttpSession();
                mockSession["UserId"] = "1";
                mockHttpContext.Setup(s => s.Session).Returns(mockSession);

                var controller = new UserController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = mockHttpContext.Object
                    }
                };

                var result = controller.GetCurrentUser();
            }
        }

        [TestMethod]
        public void GetCurrentUser_UserFound_ReturnsOkResult()
        {
            var userId = "1";
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                UserType = UserType.Student,
                PasswordHash = "null"
            };

            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDb")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.User.Add(user);
                context.SaveChanges();
            }

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["UserId"] = userId;
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);

            var controller = new UserController(new LibDbContext(options))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var result = controller.GetCurrentUser();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;

            Assert.IsNotNull(okResult);
            var model = okResult.Value.ToString();
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Contains("UserId = 1"));
            Assert.IsTrue(model.Contains("FirstName = John"));
            Assert.IsTrue(model.Contains("LastName = Doe"));
            Assert.IsTrue(model.Contains("Email = john.doe@example.com"));
            Assert.IsTrue(model.Contains("UserType = Student"));
        }

        [TestMethod]
        public void Logout_ClearsSession_ReturnsOk()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDb")
                .Options;

            using (var context = new LibDbContext(options))
            {
                var authenticationController = new UserController(context);

                var mockHttpContext = new Mock<HttpContext>();
                mockHttpContext.Setup(h => h.Session).Returns(new Mock<ISession>().Object);

                var controllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                };

                authenticationController.ControllerContext = controllerContext;

                var result = authenticationController.Logout();

                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var okResult = (OkObjectResult)result;
                var value = okResult.Value as dynamic;

                Assert.IsNotNull(value);
                Assert.AreEqual("{ Message = Logout successful }", value.ToString());

                mockHttpContext.Verify(h => h.Session.Clear(), Times.Once);
            }
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_UserDoesNotExist_ThrowsSecurityTokenException()
        {
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LibDbContext(dbContextOptions);
            var authenticationController = new UserController(context);

            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            await Assert.ThrowsExceptionAsync<SecurityTokenException>(() => authenticationController.Login(loginRequest));
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ThrowsSecurityTokenException()
        {
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LibDbContext(dbContextOptions);
            var authenticationController = new UserController(context);

            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                FirstName = "user",
                LastName = "test",
                PasswordHash = UserController.HashPassword("password123")
            };

            context.User.Add(user);
            context.SaveChanges();

            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            await Assert.ThrowsExceptionAsync<SecurityTokenException>(() => authenticationController.Login(loginRequest));
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LibDbContext(dbContextOptions);
            var userController = new UserController(context);

            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                FirstName = "user",
                LastName = "test",
                PasswordHash = UserController.HashPassword("password123")
            };

            context.User.Add(user);
            context.SaveChanges();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(h => h.Session).Returns(new Mock<ISession>().Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            userController.ControllerContext = controllerContext;

            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var result = await userController.Login(loginRequest);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("{ Message = Login successful }", ((dynamic)okResult.Value.ToString()));
        }

        [TestCleanup]
        public void CleanUp()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }

    public class FaultyDbSet<TEntity> : DbSet<TEntity> where TEntity : class
    {
        public override IEntityType EntityType => throw new NotImplementedException();

        public override IQueryable<TEntity> AsQueryable() => throw new Exception("Simulated error");
    }

    public class TestData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }
}
