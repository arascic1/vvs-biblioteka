using Microsoft.EntityFrameworkCore;
using VVS_biblioteka.Models;

namespace VVS_biblioteka
{
    public class LibDbContext: DbContext
    {
        public LibDbContext(DbContextOptions<LibDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
    }
}
