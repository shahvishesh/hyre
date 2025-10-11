using Hyre.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Hyre.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<Skill> Skills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Skill>().HasData(
                new Skill { SkillID = 1, SkillName = "JavaScript" },
                new Skill { SkillID = 2, SkillName = "C#" },
                new Skill { SkillID = 3, SkillName = "SQL" },
                new Skill { SkillID = 4, SkillName = "Node.js" },
                new Skill { SkillID = 5, SkillName = "React" },
                new Skill { SkillID = 6, SkillName = "Python" },
                new Skill { SkillID = 7, SkillName = "Angular" },
                new Skill { SkillID = 8, SkillName = "AWS" }
            );
        }
        public DbSet<Candidate> Candidates { get; set; }

    }
}
