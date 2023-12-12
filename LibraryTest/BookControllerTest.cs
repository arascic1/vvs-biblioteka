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
    





    }
}
