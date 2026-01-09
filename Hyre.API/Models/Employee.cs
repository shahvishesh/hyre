using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        public string? UserID { get; set; }  = string.Empty;

        [ForeignKey(nameof(UserID))]
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; }

        [MaxLength(150)]
        public string? Designation { get; set; }

        public DateTime JoiningDate { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; }
        // Full-time, Contract

        [MaxLength(50)]
        public string EmploymentStatus { get; set; } = "Active";
        // Active, OnNotice, Resigned, Terminated

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
