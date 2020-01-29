using StudentsFileSharingApp.Entities.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StudentsFileSharingApp.Entities.Models
{
    public class File
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public FileType FileType { get; set; }

        public DateTime DateAdded { get; set; }

        public string DrivePath { get; set; }

        public User Owner { get; set; }
    }
}