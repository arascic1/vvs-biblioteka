using System;
using System.Diagnostics;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class SortWhiteBoxTest
    {
        private Random random = new Random();

        [TestMethod]
        public void SortAlphanumerically_EmptyList_ReturnsEmptyList()
        {
            List<User> emptyList = new List<User>();

            List<User> result = UserController.SortAlphanumerically(emptyList);

            CollectionAssert.AreEqual(emptyList, result);
        }

        [TestMethod]
        public void SortAlphanumerically_SingleUserList_ReturnsSameList()
        {
            List<User> singleUserList = new List<User> { new User { LastName = "Smith", FirstName = "John" } };

            List<User> result = UserController.SortAlphanumerically(singleUserList);

            CollectionAssert.AreEqual(singleUserList, result);
        }

        [TestMethod]
        public void SortAlphanumerically_SortsCorrectly()
        {
            List<User> unsortedList = new List<User>
            {
                new User { LastName = "Doe", FirstName = "John" },
                new User { LastName = "Smith", FirstName = "Alice" },
                new User { LastName = "Doe", FirstName = "Jane" },
                new User { LastName = "Brown", FirstName = "Charlie" }
            };

            List<User> result = UserController.SortAlphanumerically(unsortedList);

            List<User> expectedList = new List<User>
            {
                new User { LastName = "Brown", FirstName = "Charlie" },
                new User { LastName = "Doe", FirstName = "Jane" },
                new User { LastName = "Doe", FirstName = "John" },
                new User { LastName = "Smith", FirstName = "Alice" }
            };

            CollectionAssert.AreEqual(expectedList, result);
        }

        [TestMethod]
        public void SortAlphanumerically_NullList_ThrowsNullReferenceException()
        {
            List<User> nullList = null;

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                List<User> result = UserController.SortAlphanumerically(nullList);
            });
        }

        [TestMethod]
        public void SortAlphanumerically_ListWithNullNames_HandlesNullNames()
        {
            List<User> userListWithNulls = new List<User>
            {
                new User { LastName = null, FirstName = "John" },
                new User { LastName = "Smith", FirstName = null },
                new User { LastName = null, FirstName = null }
            };  

            List<User> result = UserController.SortAlphanumerically(userListWithNulls);

            Assert.IsNull(result[0].LastName);
            Assert.AreEqual("John", result[1].FirstName);
            Assert.IsNull(result[1].LastName);
            Assert.IsNull(result[2].FirstName);
            Assert.AreEqual("Smith", result[2].LastName);
            Assert.IsNull(result[2].FirstName);
        }

        [TestMethod]
        public void SortAlphanumerically_LargeDataset_PerformanceTest()
        {
            List<User> largeDataset = GenerateLargeDataset(10000);

            var stopwatch = Stopwatch.StartNew();
            List<User> result = UserController.SortAlphanumerically(largeDataset);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000);
        }

        private List<User> GenerateLargeDataset(int size)
        {
            var random = new Random();
            var dataset = new List<User>();

            for (int i = 0; i < size; i++)
            {
                var user = new User
                {
                    LastName = GenerateRandomName(),
                    FirstName = GenerateRandomName()
                };
                dataset.Add(user);
            }

            return dataset;
        }

        private string GenerateRandomName()
        {
            var names = new[] { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller" };
            return names[random.Next(names.Length)];
        }
    }
}