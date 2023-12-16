using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LibraryTest
{
    [TestClass]
    public class BookTests
    {
        private BookController bookController;
        private Mock<LibDbContext> mockLibDbContext;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            mockLibDbContext = new Mock<LibDbContext>(options);
            bookController = new BookController(new LibDbContext(options));
        }

        [TestMethod]
        public async Task AddValidBookTest()
        {
            var validBook = new Book
            {
                Id = 1,
                Title = "Title 1",
                Author = "Author 1",
                Description = "Description 1",
                price = 1
            };

            var result = await bookController.AddBook(validBook) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Book added successfully!", result.Value);

        }

        [TestMethod]
        public async Task AddInvalidBookTest()
        {
            var validBook = new Book
            {
                Id = 2,
                Title = "Title 1",
                Author = "Author 1",
                Description = "Description 1",
                price = 1
            };

            var invalidBook = new Book
            {
                Id = 2,
                Title = "Title 2",
                Author = "Author 2",
                Description = "Description 2",
                price = 2
            };

            var first = await bookController.AddBook(validBook) as OkObjectResult;
            var result = await bookController.AddBook(invalidBook) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Book with the same Id already exists.", result.Value);
        }

        [TestMethod]
        public async Task AddBookInvalidModelStateTest()
        {
            var invalidBook = new Book();
            bookController.ModelState.AddModelError("Title", "Title is required");
            var result = await bookController.AddBook(invalidBook) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetInvalidBookDetailsTest()
        {
            int id = 1000;

            var result = bookController.GetBookDetails(id) as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual($"Book with ID {id} not found.", result.Value);
        }

        [TestMethod]
        public async Task GetValidBookDetailsTest()
        {
            var validBook = new Book
            {
                Id = 3,
                Title = "Title 1",
                Author = "Author 1",
                Description = "Description 1",
                price = 1
            };

            var add = await bookController.AddBook(validBook) as OkObjectResult;
            var result = bookController.GetBookDetails(validBook.Id) as OkObjectResult;

            Assert.IsInstanceOfType(result.Value, typeof(object));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LoanTest1()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var book = new Book
                {
                    Id = 2,
                    Author = "autor",
                    Description = "description",
                    price = 10,
                    Title = "title"
                };

                context.Book.Add(book);
                context.SaveChanges();
            }

            LoanRequest request = new LoanRequest
            {
                BookId = 2,
                UserId = 1
            };

            var result = bookController.LoanBook(request).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void LoanTest2()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var book = new Book
                {
                    Id = 2,
                    Author = "autor",
                    Description = "description",
                    price = 10,
                    Title = "title"
                };

                context.Book.Add(book);
                context.SaveChanges();
            }

            LoanRequest request = new LoanRequest
            {
                BookId = 2,
                UserId = 1
            };

            bookController.LoanBook(request);
            var result = bookController.LoanBook(request).GetAwaiter().GetResult();
            Assert.AreEqual(result.Message, "Book is already loaned!");
        }

        [TestMethod]
        public void LoanTest3()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var book = new Book
                {
                    Id = 2,
                    Author = "autor",
                    Description = "description",
                    price = 10,
                    Title = "title"
                };

                var book2 = new Book
                {
                    Id = 3,
                    Author = "autor",
                    Description = "description",
                    price = 10,
                    Title = "title"
                };

                context.Book.Add(book);
                context.Book.Add(book2);
                context.SaveChanges();
            }

            LoanRequest request = new LoanRequest
            {
                BookId = 2,
                UserId = 1
            };

            LoanRequest request2 = new LoanRequest
            {
                BookId = 3,
                UserId = 1
            };

            bookController.LoanBook(request);
            var result = bookController.LoanBook(request2).GetAwaiter().GetResult();
            Assert.AreEqual(result.Message, "You already loaned book!");
        }

        [TestMethod]
        public void GetBookBackTest1()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var book = new Book
                {
                    Id = 2,
                    Author = "autor",
                    Description = "description",
                    price = 10,
                    Title = "title"
                };

                context.Book.Add(book);
                context.SaveChanges();
            }

            LoanRequest request = new LoanRequest
            {
                BookId = 2,
                UserId = 1
            };

            bookController.LoanBook(request).GetAwaiter().GetResult();
            GetBookBackRequest request2 = new GetBookBackRequest { BookId = 2 };
            var response = bookController.GetBookBack(request2).GetAwaiter().GetResult();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        [TestMethod]
        public void GetBookBackTest2()
        {

            GetBookBackRequest request2 = new GetBookBackRequest { BookId = 2 };
            var response = bookController.GetBookBack(request2).GetAwaiter().GetResult();
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that id is not loaned!");

        }
    }
}
