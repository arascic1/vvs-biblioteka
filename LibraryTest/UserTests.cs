using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System.Diagnostics;
using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void Index_ShouldReturnUsersFromInMemoryDbContext()
        {
            // Arrange
            List<User> testData = new List<User>
            {
                new User { Id = 1, FirstName = "User1", LastName = "Last1", Email = "mail1@gmail.com", PasswordHash = "123" },
                new User { Id = 2, FirstName = "User2", LastName = "Last2", Email = "mail2@gmail.com", PasswordHash = "123" },
                new User { Id = 3, FirstName = "User3", LastName = "Last3", Email = "mail3@gmail.com", PasswordHash = "123" }
            };

            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.User.AddRange(testData);
                context.SaveChanges();
            }

            using (var context = new LibDbContext(options))
            {
                UserController controller = new UserController(context);

                // Act
                IEnumerable<User> result = controller.Index();

                // Assert
                Assert.IsNotNull(result);
                CollectionAssert.AreEquivalent(testData, result.ToList());
            }
        }
    }

   
}