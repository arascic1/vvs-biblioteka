using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

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
    }
}
