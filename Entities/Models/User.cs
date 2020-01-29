using StudentsFileSharingApp.Entities.Models.ManyToMany;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentsFileSharingApp.Entities.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<UserGroup> Groups { get; set; }

        public List<Post> Messages { get; set; }

        public List<PostComment> Comments { get; set; }

        public List<File> Files { get; set; }

        public string Login { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }
}