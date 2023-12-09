using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using VVS_biblioteka.Models;
using static VVS_biblioteka.Models.User;

namespace VVS_biblioteka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly LibDbContext _context;

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

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Email = req.Email,
                    PasswordHash = HashPassword(req.Password)
                    UserType=req.GetType
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest(ModelState);
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
                    switch (user.userType)
                    {
                        case Models.User.UserType.Student:
                            tip="Student";
 ;
                            break;
                        case Models.User.UserType.Ucenik:
                            tip="Ucenik";
                            ;
                            break;
                        case Models.User.UserType.Penzioner:
                            tip="Penzioner";
                            ;
                            break;
                        case Models.User.UserType.Dijete:
                            tip="Dijete";
                            ;
                            break;
                        default:
                            tip = "Nepoznato";
                            break;


                    }
                    return Ok(new
                    {
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        UserType = tip
                    });

                }
            }

            throw new HttpRequestException("User not found");
        }
        public decimal ApplyDiscount(decimal price)
        {
            
            if (UserType == UserType.Student)
            {
                return price * 0.9; 
            }
            else if (UserType==UserType.Ucenik)
            {
                return price*0.8;
            }
            else if(UserType==UserType.Penzioner || UserType==UserType.Dijete)
            {
                return price*0.95;
            }
           
            else
            {
                return 
                   price; 
            }
        }
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

        private static List<User> SortAlphanumerically(List<User> users)
        {
            for (int i = 0; i < users.Count - 1; i++)
            {
                for (int j = 0; j < users.Count - i - 1; j++)
                {
                    // Compare last names
                    if (string.Compare(users[j].LastName, users[j + 1].LastName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // Swap users
                        var temp = users[j];
                        users[j] = users[j + 1];
                        users[j + 1] = temp;
                    }
                    // If last names are equal, compare first names
                    else if (string.Compare(users[j].LastName, users[j + 1].LastName, StringComparison.OrdinalIgnoreCase) == 0 &&
                             string.Compare(users[j].FirstName, users[j + 1].FirstName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // Swap users
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
            // Validate that the name contains only letters
            return !string.IsNullOrWhiteSpace(name) && name.All(char.IsLetter);
        }

        private bool IsValidEmailDomain(string email)
        {
            // Validate email domain
            string[] allowedDomains = { "gmail.com", "etf.unsa.ba", "yahoo.com", "outlook.com" };
            string domain = email.Split('@').LastOrDefault()?.ToLower();
            return domain != null && allowedDomains.Contains(domain);
        }
       



        public LibDbContext GetContext()
        {
            return _context;
        }
    }
}
