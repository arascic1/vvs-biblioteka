using Microsoft.EntityFrameworkCore;
using Moq;
using VVS_biblioteka.Controllers;
using VVS_biblioteka;
using VVS_biblioteka.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryTest
{
    [TestClass]
    public class UserControllerTest
    {
        private UserController userController;
        private Mock<LibDbContext> mockLibDbContext;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            mockLibDbContext = new Mock<LibDbContext>(options);
            userController = new UserController(new LibDbContext(options));

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

    }
}
