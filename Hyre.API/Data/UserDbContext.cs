using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}
