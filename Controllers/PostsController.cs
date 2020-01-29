﻿using Microsoft.AspNetCore.Mvc;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using System;
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

            await context.SaveChangesAsync();

            return NoContent();
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
                Tag = record.Title,
                Group = groupRecord
            };

            context.Set<Post>().Add(entity);

            await context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = record.Id }, record);
        }
    }
}