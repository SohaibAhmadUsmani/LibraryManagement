using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        // --- DB COLUMNS (New) ---
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public string Status { get; set; } = "Pending";
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- TRANSLATORS (Fixes your View Errors) ---
        [NotMapped] public string FullName { get => Name; set => Name = value; }
        [NotMapped] public DateTime CreatedDate { get => CreatedAt; set => CreatedAt = value; }
        [NotMapped] public bool IsActive => Status == "Active";

        [NotMapped]
        public int AccessLevel
        {
            get => Role == "Admin" ? 1 : (Role == "Librarian" ? 2 : 3);
            set => Role = value == 1 ? "Admin" : (value == 2 ? "Librarian" : "Student");
        }

        // Fix for "ConfirmPassword" error in Register View
        [NotMapped] public string ConfirmPassword { get; set; } = string.Empty;
    }
}