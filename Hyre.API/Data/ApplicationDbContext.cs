using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<Skill> Skills { get; set; }
        
        public DbSet<Candidate> Candidates { get; set; }

    }
}
