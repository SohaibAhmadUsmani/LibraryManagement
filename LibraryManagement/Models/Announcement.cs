using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    // This class maps to the 'Announcements' table you created in SQL
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Type { get; set; } // e.g., "General" or "Policy"

        public DateTime PostedDate { get; set; } = DateTime.Now;
    }
}