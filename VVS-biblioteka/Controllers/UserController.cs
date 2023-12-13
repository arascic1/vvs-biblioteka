using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase, IUserController
    {
        private readonly LibDbContext _context;
        public UserController() { }

        public UserController(LibDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<User> Index()
        {
            return _context.User.ToList();
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<User?> Details(int id)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            if (req.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long.");
            }

            if (!IsValidName(req.FirstName) || !IsValidName(req.LastName))
            {
                throw new ArgumentException("First name and last name can only contain letters.");
            }

            if (!IsValidEmailDomain(req.Email))
            {
                throw new ArgumentException("Invalid email domain. Allowed domains are gmail.com, etf.unsa.ba, yahoo.com, or outlook.com.");
            }
            if (!IsValidType(req.UserType))
            {
                throw new ArgumentException("Invalid type of user!");
            }


            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Email = req.Email,
                    PasswordHash = HashPassword(req.Password),
                    ExpirationDate = DateTime.Now.AddMonths(12),
                    UserType = req.UserType
                };


                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("getLoanedBooks/{userId}")]
        public IActionResult GetLoanedBooks(int userId)
        {
            var loansForUser = _context.Loan
                .Where(l => l.UserId == userId)
                .ToList();

            if (loansForUser.Count == 0)
            {
                return NotFound($"No loaned books found for user with ID {userId}.");
            }

            var loanedBooks = new List<Book>();
            foreach (var loan in loansForUser)
            {
                var book = _context.Book.FirstOrDefault(b => b.Id == loan.BookId);
                if (book != null)
                {
                    loanedBooks.Add(book);
                }
            }

            return Ok(loanedBooks);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user != null && VerifyPassword(req.Password, user.PasswordHash))
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                return Ok(new { Message = "Login successful" });
            }

            throw new SecurityTokenException("Invalid email or password");
        }

        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Logout successful" });

        }

        [HttpGet]



        [HttpGet]
        [Route("currentuser")]
        public IActionResult GetCurrentUser()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId != null && int.TryParse(userId, out int id))
            {
                var user = _context.User.FirstOrDefault(u => u.Id == id);

                if (user != null)
                {
                    string tip = "";
                    switch (user.UserType)
                    {
                        case UserType.Student:
                            tip = "Student";
                            ;
                            break;
                        case UserType.Ucenik:
                            tip = "Ucenik";
                            ;
                            break;
                        case UserType.Penzioner:
                            tip = "Penzioner";
                            ;
                            break;
                        case UserType.Dijete:
                            tip = "Dijete";
                            ;
                            break;
                        default:
                            tip = "Nepoznato";
                            break;
                    }

                    return Ok(new
                    {
                        UserId = user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        UserType = tip
                    });
                }
            }

            throw new HttpRequestException("User not found");
        }

        [HttpGet]
        [Route("search")]
        public IActionResult SearchUsers([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                    throw new ArgumentException("Search keyword cannot be empty.");

                var users = _context.User
                    .Where(u => EF.Functions.Like(u.FirstName, $"%{keyword}%") || EF.Functions.Like(u.LastName, $"%{keyword}%"))
                    .ToList();

                users = SortAlphanumerically(users);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error searching users.", Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("membershiprenewal")]

        public async Task<IActionResult> RenewMembership([FromBody] RenewalRequest req)
        {
            if (req.Months < 1) throw new ArgumentException("Number of months must be greater than 0.");
            var user = _context.User.FirstOrDefault(u => u.Id == req.UserId);
            if (user == null) throw new HttpRequestException("User not found.");
            user.ExpirationDate = user.ExpirationDate.AddMonths(req.Months);
            _context.User.Update(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private static List<User> SortAlphanumerically(List<User> users)
        {
            for (int i = 0; i < users.Count - 1; i++)
            {
                for (int j = 0; j < users.Count - i - 1; j++)
                {
                    if (string.Compare(users[j].LastName, users[j + 1].LastName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        var temp = users[j];
                        users[j] = users[j + 1];
                        users[j + 1] = temp;
                    }
                    else if (string.Compare(users[j].LastName, users[j + 1].LastName, StringComparison.OrdinalIgnoreCase) == 0 &&
                             string.Compare(users[j].FirstName, users[j + 1].FirstName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        var temp = users[j];
                        users[j] = users[j + 1];
                        users[j + 1] = temp;
                    }
                }
            }

            return users;
        }

        private static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return HashPassword(inputPassword) == hashedPassword;
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }


        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.All(char.IsLetter);
        }
        private bool IsValidType(UserType userType)
        {
            return userType==UserType.Student || userType==UserType.Penzioner || userType==UserType.Dijete || userType==UserType.Ucenik;
        }

        private bool IsValidEmailDomain(string email)
        {
            string[] allowedDomains = { "gmail.com", "etf.unsa.ba", "yahoo.com", "outlook.com" };
            string domain = email.Split('@').LastOrDefault()?.ToLower();
            return domain != null && allowedDomains.Contains(domain);
        }
    }

    public interface IUserController
    {
        IActionResult GetLoanedBooks(int userId);
    }
}
