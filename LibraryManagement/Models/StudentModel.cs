using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class StudentModel
    {
        public int StudentId { get; set; }

        [Required]
        public string StudentName { get; set; } = string.Empty; // Fixed Null Warning

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Fixed Null Warning

        public string Phone { get; set; } = string.Empty; // Fixed Null Warning

        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}