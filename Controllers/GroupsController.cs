using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Entities.Models.Enums;
using StudentsFileSharingApp.Entities.Models.ManyToMany;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentsFileSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class GroupsController : BaseController
    {
        private readonly BasicContext context;

        public GroupsController(BasicContext context)
        {
            this.context = context;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var record = await context.Set<Group>().FindAsync(id);

            if (record == null)
                return NotFound();

            var userId = GetUserId();

            if (!record.Members.Any(a => a.UserId == userId && a.UserRank == UserRank.Leader))
                return BadRequest();

            context.Set<Group>().Remove(record);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetGroup(int id)
        {
            if (!await context.Set<Group>().AnyAsync(a => a.Id == id))
                return NotFound();

            var userId = GetUserId();

            var dbResult = await context.Set<Group>()
                .Include(a => a.Posts)
                .ThenInclude(b => b.Comments)
                .FirstOrDefaultAsync(a => a.Id == id);

            var result = new GroupDto
            {
                Id = dbResult.Id,
                Name = dbResult.Name,
                Posts = dbResult.Posts.Select(a => new PostDto
                {
                    Id = a.Id,
                    AuthorName = a.Author.Name,
                    Title = a.Tag,
                    IsAuthor = a.AuthorId == userId,
                    Content = a.Content,
                    DateAdded = a.DateAdded,
                    Comments = a.Comments.Select(b => new PostCommentDto
                    {
                        Id = b.Id,
                        AuthorName = b.Author.Name,
                        IsAuthor = b.AuthorId == userId,
                        Content = b.Content,
                        DateAdded = b.DateAdded
                    }).ToList()
                }).ToList()
            };

            return result;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
        {
            var records = await context.Set<Group>().ToListAsync();

            return Ok(records.Select(a => new GroupDto { Id = a.Id, Name = a.Name }));
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> JoinGroup(int id)
        {
            var userId = GetUserId();

            var groupRecord = await context.Set<Group>().FindAsync(id);
            var userRecord = await context.Set<User>().FindAsync(userId);

            if (groupRecord == null || userRecord == null)
                return NotFound();

            groupRecord.Members.Add(new UserGroup
            {
                User = userRecord,
                UserRank = UserRank.Normal
            });

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> PostGroup(GroupDto record)
        {
            if (record == null)
                return BadRequest();

            var userId = GetUserId();

            var userRecord = await context.Set<User>().FindAsync(userId);

            if (userRecord == null)
                return BadRequest();

            if (context.Set<Group>().Any(a => a.Name.Equals(record.Name)))
                return BadRequest($"Group with name {record.Name} already exists");

            var entity = new Group
            {
                Name = record.Name,
                Members = new List<UserGroup>
                {
                    new UserGroup
                    {
                        User = userRecord,
                        UserRank = UserRank.Leader
                    }
                }
            };

            context.Set<Group>().Add(entity);

            await context.SaveChangesAsync();

            return CreatedAtAction("GetGroup", new { id = record.Id }, record);
        }
    }
}