using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VVS_biblioteka.Models;

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
            if (user == null) return null;
            return user;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Email = req.Email,
                    PasswordHash = HashPassword(req.Password)
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest(ModelState);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
