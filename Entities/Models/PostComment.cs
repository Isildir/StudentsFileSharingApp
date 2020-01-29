using System;
using System.ComponentModel.DataAnnotations;

namespace StudentsFileSharingApp.Entities.Models
{
    public class PostComment
    {
        [Key]
        public int Id { get; set; }

        public User Author { get; set; }

        public int AuthorId { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}