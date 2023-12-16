using Microsoft.EntityFrameworkCore;
using Moq;
using VVS_biblioteka.Controllers;
using VVS_biblioteka;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class BookReviewTDD
    {
        private BookController _bookController;
        private Mock<LibDbContext> mockLibDbContext;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            mockLibDbContext = new Mock<LibDbContext>(options);
            _bookController = new BookController(new LibDbContext(options));

            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };

            _bookController.AddBook(book);

        }
        [TestMethod]

        public async Task AddBookReviewTest1()
        {
            BookReview review = new BookReview
            {
                BookReviewId = 1,
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            AddBookReviewResponse response = _bookController.AddBookReview(review).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");

        }

        [TestMethod]
        public async Task AddBookReviewTest2()
        {
            BookReview review = new BookReview
            {

                BookId = 111,
                Grade = 4,
                Message = "Test"
            };

            AddBookReviewResponse response = _bookController.AddBookReview(review).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");

        }

        [TestMethod]
        public void AddBookReviewTest3()
        {
            BookReview review = new BookReview
            {

                BookId = 11,
                Grade = 0,
                Message = "Test"
            };

            AddBookReviewResponse response = _bookController.AddBookReview(review).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be less than 1!");

        }

        [TestMethod]
        public void AddBookReviewTest4()
        {

            BookReview review = new BookReview
            {

                BookId = 11,
                Grade = 6,
                Message = "Test"
            };


            AddBookReviewResponse response = _bookController.AddBookReview(review).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be greater than 5!");

        }

        [TestMethod]
        public void AverageBookReviewTest1()
        {
            BookReview review1 = new BookReview
            {

                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            BookReview review2 = new BookReview
            {

                BookId = 11,
                Grade = 5,
                Message = "Test"
            };

            _bookController.AddBookReview(review1);
            _bookController.AddBookReview(review2);

            GetAverageGradeResponse response = _bookController.GetAverageGrade(11).GetAwaiter().GetResult();


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
            Assert.IsTrue(response.Value <= 4.5 && response.Value >= 4);
        }

        [TestMethod]
        public void AverageBookReviewTest2()
        {
            GetAverageGradeResponse response = _bookController.GetAverageGrade(1).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");
            Assert.AreEqual(response.Value, -1);
        }

        [TestMethod]
        public void DeleteBookReviewTest1()
        {
            BookReview review = new BookReview
            {

                BookId = 11,
                Grade = 4,
                Message = "Test"
            };


            _bookController.AddBookReview(review);
            DeleteReviewResponse response = _bookController.DeleteReview(1).GetAwaiter().GetResult();


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
        }

        [TestMethod]
        public void DeleteBookReviewTest2()
        {
            DeleteReviewResponse response = _bookController.DeleteReview(111).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Review with that Id is not found!");
        }
    }
}

