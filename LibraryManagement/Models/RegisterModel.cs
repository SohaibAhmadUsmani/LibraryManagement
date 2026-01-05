using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public string FullName { get; set; }
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; } // "Student" or "Librarian"
    }
}