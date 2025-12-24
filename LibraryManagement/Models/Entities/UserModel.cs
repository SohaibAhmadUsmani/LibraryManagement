using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("Users")]
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // The Real Database Value
        public int AccessLevel { get; set; }

        // THE FIX: A Read/Write "Role" property that bridges the gap
        [NotMapped]
        public string Role
        {
            get
            {
                if (AccessLevel == 3) return "Admin";
                if (AccessLevel == 2) return "Librarian";
                return "Student";
            }
            set
            {
                // This allows your old code (user.Role = "...") to work!
                if (value == "Admin") AccessLevel = 3;
                else if (value == "Librarian") AccessLevel = 2;
                else AccessLevel = 1;
            }
        }

        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}