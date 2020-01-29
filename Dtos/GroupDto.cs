using System.Collections.Generic;

namespace StudentsFileSharingApp.Dtos
{
    public class GroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<PostDto> Posts { get; set; }
    }
}