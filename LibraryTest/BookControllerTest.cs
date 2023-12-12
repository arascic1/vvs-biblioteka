using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;



namespace LibraryTest
{

    [TestClass]
    public class BookControllerTest
    {
       
        [TestMethod]
        public void ApplyCategorySpecificBenefits_StudentsCategory_ShouldSetDiscountAndDays()
        {
            var user = new User { UserType = UserType.Student };
            var loan = new Loan { Price = 100, Days = 0 }; 

            var bookController = new BookController(null); 
            
            bookController.ApplyCategorySpecificBenefits(user, loan);
           
            Assert.AreEqual(70, loan.Price); // CalculateDiscountedFee(100, 30) returns 70
            Assert.AreEqual(60, loan.Days);
        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_UcenikCategory_ShouldSetDiscountAndDays()
        {
            var user = new User { UserType=UserType.Ucenik };
            var loan = new Loan { Price=100, Days=20 };


            var bookController = new BookController(null);


            bookController.ApplyCategorySpecificBenefits(user, loan);


            Assert.AreEqual(90, loan.Price); // CalculateDiscountedFee(100, 30) returns 90
            Assert.AreEqual(15, loan.Days);
        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_PenzionerCategory_ShouldSetDiscountAndDays()
        {
            var user = new User { UserType=UserType.Penzioner };
            var loan = new Loan { Price=100, Days=0 };
            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(85, loan.Price);
            Assert.AreEqual(30, loan.Days);

        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DijeteCategory_ShouldSetDiscountAndDays()
        {
            var user = new User { UserType=UserType.Dijete };
            var loan = new Loan { Price=100, Days=0 };
            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(95, loan.Price);
            Assert.AreEqual(10, loan.Days);

        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DifferentValues()
        {
            var user = new User { UserType = UserType.Student };
            var loan = new Loan { Price = 200, Days = 15 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(140, loan.Price); 
            Assert.AreEqual(60, loan.Days);
        }




        [TestMethod]
        public async Task DeleteBook_WithExistingBook_ReturnsOk()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            using (var context = new LibDbContext(dbContextOptions))
            {
                var bookToAdd = new Book { Id = 1, Title = "Test Book" ,
                    Author = "Author 1",
                    Description = "Description 1"
                };
                await context.Book.AddAsync(bookToAdd);
                await context.SaveChangesAsync();
            }

            var controller = new BookController(new LibDbContext(dbContextOptions));

            
            var result = await controller.DeleteBook(1) as OkObjectResult;

           
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

           
            Assert.AreEqual($"Book with ID 1 deleted successfully.", result.Value);
        }

        [TestMethod]
        public async Task DeleteBook_WithExistingBook_ReturnsNotOk()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;



            using (var context = new LibDbContext(dbContextOptions))
            {
                var bookToAdd = new Book { Id = 3, Title = "meditacije", Author="Julia", 
                Description="Veri nice"};
                await context.Book.AddAsync(bookToAdd);
                await context.SaveChangesAsync();
            }

            var controller = new BookController(new LibDbContext(dbContextOptions));

            // Act
            var result = await controller.DeleteBook(999) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);

            Assert.AreEqual("Book with ID 999 not found.", result.Value);
        }
       

        [TestMethod]
        public async Task AddBook_ReturnsNotOk()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;



            var controller = new BookController(new LibDbContext(dbContextOptions));

            var newBook = new Book { Id = 2, Title = "Dervis i smrt", Author="Mesa Selimovic", Description="opis" };

            // Act
            var result = await controller.AddBook(newBook) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Očekujemo poruku "Book added successfully!"
            Assert.AreEqual("Book added successfully!", result.Value);
        }
        [TestMethod]
        public async Task SearchBook_IsFound()
        {
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
               .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
               .Options;
            var controller = new BookController(new LibDbContext(dbContextOptions));
            using (var context = new LibDbContext(dbContextOptions))
            {
                var booksToAdd = new List<Book>
            {
                new Book { Id = 4, Title = "Book1", Author = "Author1", Description="First Book" },
               
            };
                context.Book.AddRange(booksToAdd);
                context.SaveChanges();
            }

 

            var result = controller.SearchBooks(null, null) as OkObjectResult;

           
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var books = result.Value as List<Book>;
            Assert.IsNotNull(books);
            Assert.AreEqual(3, books.Count);

        }

        [TestMethod]
        public async Task GetBookBack_WithExistingLoan_ReturnsOk()
        {
            
            var dbContextOptions = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            using (var context = new LibDbContext(dbContextOptions))
            {
                
                var loanedBook = new Loan { BookId = 1 };
                await context.Loan.AddAsync(loanedBook);
                await context.SaveChangesAsync();
            }

            var controller = new BookController(new LibDbContext(dbContextOptions));
            var request = new GetBookBackRequest { BookId = 1 };

            
            var result = await controller.GetBookBack(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.Success);
            
            Assert.AreEqual("You got book back!", result.Message);

           
            using (var context = new LibDbContext(dbContextOptions))
            {
                var loanedBookInDatabase = context.Loan.FirstOrDefault(l => l.BookId == request.BookId);
                Assert.IsNull(loanedBookInDatabase, "Loaned book should not exist in the Loan table.");
            }
        }

       









    }
}
