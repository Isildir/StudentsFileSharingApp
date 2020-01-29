using Microsoft.AspNetCore.Mvc;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using System;
using System.Threading.Tasks;

namespace StudentsFileSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : BaseController
    {
        private readonly BasicContext context;

        public CommentsController(BasicContext context)
        {
            this.context = context;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = GetUserId();

            var record = await context.Set<PostComment>().FindAsync(id);

            if (record == null)
                return NotFound();

            if (record.AuthorId != userId)
                return BadRequest();

            context.Set<PostComment>().Remove(record);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<PostCommentDto>> PostComment(PostCommentDto record)
        {
            if (record == null)
                return BadRequest();

            var userId = GetUserId();

            var userRecord = await context.Set<User>().FindAsync(userId);
            var postRecord = await context.Set<Post>().FindAsync(record.PostId);

            if (userRecord == null || postRecord == null)
                return BadRequest();

            var entity = new PostComment
            {
                Post = postRecord,
                Author = userRecord,
                DateAdded = DateTime.UtcNow,
                Content = record.Content
            };

            context.Set<PostComment>().Add(entity);

            await context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = record.Id }, record);
        }
    }
}