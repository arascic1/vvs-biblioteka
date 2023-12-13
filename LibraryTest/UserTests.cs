using Microsoft.EntityFrameworkCore;

using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class UserTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var _options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(_options))
            {
                context.User.RemoveRange(context.User);
                context.SaveChanges();
            }
        }

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

                IEnumerable<User> result = controller.Index();

                Assert.IsNotNull(result);
                CollectionAssert.AreEquivalent(testData, result.ToList());
            }
        }

        [TestMethod]
        public void EqualUsersTest()
        {
            var user1 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword",
                UserType = UserType.Student
            };

            var user2 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword",
                UserType = UserType.Student
            };

            Assert.IsTrue(user1.Equals(user2));
            Assert.IsTrue(user2.Equals(user1));
        }

        [TestMethod]
        public void NotEqualUsersTest()
        {
            var user1 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword",
                UserType = UserType.Ucenik
            };

            var user2 = new User
            {
                Id = 2, 
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PasswordHash = "differenthashedpassword",
                UserType = UserType.Student
            };

            Assert.IsFalse(user1.Equals(user2));
            Assert.IsFalse(user2.Equals(user1));
        }
    }

   
}