using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VVS_biblioteka;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class NotificationsTDD
    {
        [TestMethod]
        public void SendNotification_ValidUserAndMessage_NotificationSentSuccessfully()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var notificationService = new NotificationController(context);

                var user = new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@gmail.com",
                    PasswordHash = "hashedPassword",
                };

                context.User.Add(user);
                context.SaveChanges();

                var result = notificationService.SendNotification(user.Id, "Test Notification");
                var goal = ((Microsoft.AspNetCore.Mvc.ObjectResult)result.Result).Value.ToString();

                Assert.IsTrue(goal.Contains("Notification sent successfully"));
                Assert.AreEqual("Test Notification", context.Notification.Last().Message);
            }
        }

        [TestMethod]
        public void ReceiveNotification_UserHasUnreadNotifications_NotificationsReceivedSuccessfully()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var notificationService = new NotificationController(context);

                var user = new User
                {
                    Id = 1,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@gmail.com",
                    PasswordHash = "hashedPassword",
                };

                var notification = new Notification
                {
                    Id = 1,
                    UserId = user.Id,
                    Message = "New Message",
                    IsRead = false
                };

                context.User.Add(user);
                context.Notification.Add(notification);
                context.SaveChanges();

                var unreadNotifications = (List<Notification>)notificationService.GetUnreadNotifications(user.Id);

                Assert.IsNotNull(unreadNotifications);
                Assert.AreEqual(1, unreadNotifications.Count);
                Assert.IsFalse(unreadNotifications[0].IsRead);
            }
        }

        [TestMethod]
        public void MarkNotificationAsRead_ValidNotificationId_NotificationMarkedAsRead()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var notificationService = new NotificationController(context);

                var user = new User
                {
                    Id = 1,
                    FirstName = "Bob",
                    LastName = "Doe",
                    Email = "bob.doe@gmail.com",
                    PasswordHash = "hashedPassword",
                };

                var notification = new Notification
                {
                    Id = 1,
                    UserId = user.Id,
                    Message = "Read Me",
                    IsRead = false
                };

                context.User.Add(user);
                context.Notification.Add(notification);
                context.SaveChanges();

                var result = (OkObjectResult)notificationService.MarkNotificationAsRead(user.Id, notification.Id).GetAwaiter().GetResult();

                Assert.AreEqual(200, result.StatusCode);
                Assert.IsTrue(context.Notification.Find(notification.Id).IsRead);
            }
        }

        [TestMethod]
        public void DeleteNotification_ValidNotificationId_NotificationDeletedSuccessfully()
        {
            var options = new DbContextOptionsBuilder<LibDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new LibDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var notificationService = new NotificationController(context);

                var user = new User
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Doe",
                    Email = "alice.doe@gmail.com",
                    PasswordHash = "hashedPassword",
                };

                var notification = new Notification
                {
                    Id = 1,
                    UserId = user.Id,
                    Message = "Delete Me",
                    IsRead = false
                };

                context.User.Add(user);
                context.Notification.Add(notification);
                context.SaveChanges();

                var result = (OkObjectResult)notificationService.DeleteNotification(user.Id, notification.Id).GetAwaiter().GetResult();

                Assert.AreEqual(200, result.StatusCode);
                Assert.AreEqual(0, context.Notification.Count());
            }
        }
    }
}