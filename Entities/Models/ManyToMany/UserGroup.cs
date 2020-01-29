using StudentsFileSharingApp.Entities.Models.Enums;

namespace StudentsFileSharingApp.Entities.Models.ManyToMany
{
    public class UserGroup
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public int GroupId { get; set; }

        public Group Group { get; set; }

        public UserRank UserRank { get; set; }
    }
}