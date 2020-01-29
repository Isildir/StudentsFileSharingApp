using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentsFileSharingApp.Dtos
{
    public class PostCommentDto
    {
        public int Id { get; set; }

        public bool IsAuthor { get; set; }

        public string Content { get; set; }

        public string AuthorName { get; set; }

        public DateTime DateAdded { get; set; }

        [NotMapped]
        public int PostId { get; set; }
    }
}