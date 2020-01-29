﻿using StudentsFileSharingApp.Entities.Models.ManyToMany;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentsFileSharingApp.Entities.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<UserGroup> Members { get; set; }

        public List<File> Files { get; set; }

        public List<Message> ChatMessages { get; set; }
    }
}