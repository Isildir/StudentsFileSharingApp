using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentsFileSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : BaseController
    {
        private readonly BasicContext context;

        public PostsController(BasicContext context)
        {
            this.context = context;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var record = await context.Set<Post>().FindAsync(id);

            if (record == null)
                return NotFound();

            var userId = GetUserId();

            if (record.AuthorId != userId)
                return BadRequest();

            context.Set<Post>().Remove(record);
            context.Set<PostComment>().RemoveRange(context.Set<PostComment>().Where(a => a.PostId == id));

            try
            {
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch(DbUpdateException ex)
            {
                Logger.Log($"{nameof(PostsController)} {nameof(DeletePost)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest();
            }
            catch(Exception ex)
            {
                Logger.Log($"{nameof(PostsController)} {nameof(DeletePost)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> PostPost(PostDto record)
        {
            if (record == null)
                return BadRequest();

            var userId = GetUserId();

            var userRecord = await context.Set<User>().FindAsync(userId);
            var groupRecord = await context.Set<Group>().FindAsync(record.GroupId);

            if (userRecord == null || groupRecord == null)
                return BadRequest();

            var entity = new Post
            {
                Author = userRecord,
                DateAdded = DateTime.UtcNow,
                Content = record.Content,
                Title = record.Title,
                Group = groupRecord
            };

            context.Set<Post>().Add(entity);

            try
            {
                await context.SaveChangesAsync();

                return Ok(new PostDto
                {
                    AuthorName = entity.Author.Name,
                    DateAdded = entity.DateAdded,
                    Id = entity.Id,
                    IsAuthor = true,
                    Title = entity.Title,
                    Content = entity.Content,
                    Comments = new List<PostCommentDto>()
                });
            }
            catch (DbUpdateException ex)
            {
                Logger.Log($"{nameof(PostsController)} {nameof(PostPost)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest();
            }
            catch (Exception ex)
            {
                Logger.Log($"{nameof(PostsController)} {nameof(PostPost)}", ex.Message, NLog.LogLevel.Warn, ex);

                return BadRequest();
            }
        }
    }
}