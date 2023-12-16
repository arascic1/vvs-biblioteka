using Microsoft.EntityFrameworkCore;
using VVS_biblioteka.Models;

namespace VVS_biblioteka
{
    public class LibDbContext : DbContext
    {
        public LibDbContext(DbContextOptions<LibDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        
        public DbSet<Book> Book {  get; set; }
        public DbSet<Loan> Loan { get; set; }

        public DbSet<Notification> Notification { get; set; }

        public DbSet<BookReview> BookReview { get; set; }
    }
}
