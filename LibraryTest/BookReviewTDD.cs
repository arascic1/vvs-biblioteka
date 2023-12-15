using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
        [TestMethod]

        public void AddBookReviewTest1()
        {
            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };


            BookReview review = new BookReview
            {BookReviewId = 1,
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            _bookController.AddBook(book);
            AddBookReviewResponse response = _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
            
        }

        public void AddBookReviewTest2()
        {
            BookReview review = new BookReview
            {BookReviewId = 1,
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            
            AddBookReviewResponse response = _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");

        }
        public void AddBookReviewTest3()
        {
            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };


            BookReview review = new BookReview
            {   
                BookReviewId = 1,  
                BookId = 11,
                Grade = 0,
                Message = "Test"
            };

            _bookController.AddBook(book);
            AddBookReviewResponse response = _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be less than 1!");

        }
        public void AddBookReviewTest4()
        {
            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };


            BookReview review = new BookReview
            {
                BookReviewId = 1,
                BookId = 11,
                Grade = 6,
                Message = "Test"
            };

            _bookController.AddBook(book);
            AddBookReviewResponse response = _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be greater than 5!");

        }
        public void AverageBookReviewTest1()
        {
            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };


            BookReview review1 = new BookReview
            {
                BookReviewId = 1,
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            BookReview review2 = new BookReview
            {
                BookReviewId = 2,
                BookId = 11,
                Grade = 5,
                Message = "Test"
            };

            _bookController.AddBook(book);
            _bookController.AddBookReview(review1);
            _bookController.AddBookReview(review2);

            GetAverageGradeResponse response = _bookController.GetAverageGrade(11);


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
            Assert.AreEqual(response.Value, 4.5);
        }
        public void AverageBookReviewTest2()
        {
            GetAverageGradeResponse response = _bookController.GetAverageGrade(1);


            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");
            Assert.IsNull(response.Value);
        }
        public void DeleteBookReviewTest1()
        {
            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                price = 5

            };


            BookReview review = new BookReview
            {
                BookReviewId = 1,
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            _bookController.AddBook(book);
            _bookController.AddBookReview(review);
            DeleteReviewResponse response = _bookController.DeleteReview(1);


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
        }
        public void DeleteBookReviewTest2()
        {
            DeleteReviewResponse response = _bookController.DeleteReview(111);


            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Review with that Id is not found!");
        }
    }
}
