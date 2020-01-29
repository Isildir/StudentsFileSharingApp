using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentsFileSharingApp.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }

        public bool IsAuthor { get; set; }

        public string Title { get; set; }

        public List<PostCommentDto> Comments { get; set; }

        public string Content { get; set; }

        public string AuthorName { get; set; }

        public DateTime DateAdded { get; set; }

        [NotMapped]
        public int GroupId { get; set; }
    }
}