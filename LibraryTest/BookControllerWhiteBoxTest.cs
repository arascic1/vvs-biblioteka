using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVS_biblioteka.Controllers;
using VVS_biblioteka.Models;

namespace LibraryTest
{
    [TestClass]
    public class BookControllerWhiteBoxTest
    {
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Student()
        {
            User user = new User { UserType=UserType.Student };
            Loan loan = new Loan { Price=10, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(7, loan.Price); 
            Assert.AreEqual(60, loan.Days);


        }

        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Ucenik()
        {
            User user = new User { UserType=UserType.Ucenik };
            Loan loan = new Loan { Price=10, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(9, loan.Price);
            Assert.AreEqual(15, loan.Days);


        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Pezioner()
        {
            User user = new User { UserType=UserType.Penzioner };
            Loan loan = new Loan { Price=100, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(85, loan.Price);
            Assert.AreEqual(30, loan.Days);


        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_DiscountAppliedDaysUpdated_Dijete()
        {
            User user = new User { UserType=UserType.Dijete };
            Loan loan = new Loan { Price=100, Days=0 };

            var bookController = new BookController(null);

            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(95, loan.Price);
            Assert.AreEqual(10, loan.Days);


        }
        private const int DefaultLoanDays = 5;
        [TestMethod]
        public void ApplyCategorySpecificBenefits_UnknownUserType_NoDiscountApplied()
        {
            
            User user = new User { UserType = (UserType)10 };// imamo samo 4 tipa tako da je ovo nepoznat tip
            Loan loan = new Loan { Price = 100, Days = 5 };

            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreEqual(100, loan.Price); // cijena se ne smije promijeniti
            Assert.AreEqual(DefaultLoanDays, loan.Days); // po defaultu
        }
        [TestMethod]
        public void ApplyCategorySpecificBenefits_Student()
        {

            User user = new User { UserType = UserType.Student };
            Loan loan = new Loan { Price = 100, Days = 0 };

            var bookController = new BookController(null);
            bookController.ApplyCategorySpecificBenefits(user, loan);

            Assert.AreNotEqual(100, loan.Price); 
            Assert.AreNotEqual(100, loan.Days); 
        }



    }
}
