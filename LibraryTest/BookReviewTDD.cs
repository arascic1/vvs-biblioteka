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

            Book book = new Book
            {
                Id = 11,
                Title = "Test",
                Author = "Test",
                Description = "Test",
                Price = 5

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

            
            AddBookReviewResponse response =await _bookController.AddBookReview(review);

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

            
            AddBookReviewResponse response = await _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");

        }
        [TestMethod]
        public async Task AddBookReviewTest3()
        {
           

            BookReview review = new BookReview
            {   
                
                BookId = 11,
                Grade = 0,
                Message = "Test"
            };

           
            AddBookReviewResponse response = await _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be less than 1!");

        }
        [TestMethod]
        public async Task AddBookReviewTest4()
        {
            
            BookReview review = new BookReview
            {
                
                BookId = 11,
                Grade = 6,
                Message = "Test"
            };

           
            AddBookReviewResponse response = await _bookController.AddBookReview(review);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Grade cannot be greater than 5!");

        }
        [TestMethod]
        public async Task AverageBookReviewTest1()
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

            
            await _bookController.AddBookReview(review1);
            await _bookController.AddBookReview(review2);

            GetAverageGradeResponse response = await _bookController.GetAverageGrade(11);


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
            Assert.IsTrue(response.Value<=4.5 && response.Value>=4);
        }
        [TestMethod]
        public async Task AverageBookReviewTest2()
        {
            GetAverageGradeResponse response = await _bookController.GetAverageGrade(1);


            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Book with that Id is not found!");
            Assert.AreEqual(response.Value,-1);
        }
        [TestMethod]
        public async Task DeleteBookReviewTest1()
        {
            


            BookReview review = new BookReview
            {
               
                BookId = 11,
                Grade = 4,
                Message = "Test"
            };

            
            await _bookController.AddBookReview(review);
            DeleteReviewResponse response = await _bookController.DeleteReview(1);


            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.Message, "Ok!");
        }
        [TestMethod]
        public async Task DeleteBookReviewTest2()
        {
            DeleteReviewResponse response = await _bookController.DeleteReview(111);


            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.Message, "Review with that Id is not found!");
        }
    }
}
