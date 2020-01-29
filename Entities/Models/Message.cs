using System;

namespace StudentsFileSharingApp.Entities.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public Group Group { get; set; }

        public User Author { get; set; }

        public DateTime DateAdded { get; set; }

        public string Tag { get; set; }
    }
}