﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentsFileSharingApp.Entities.Models
{
    public class Post
    {
        public Post()
        {
            Comments = new List<PostComment>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public int GroupId { get; set; }

        public Group Group { get; set; }

        public int AuthorId { get; set; }

        public User Author { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public string Title { get; set; }

        public List<PostComment> Comments { get; set; }
    }
}