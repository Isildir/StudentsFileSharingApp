using HeyRed.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Entities.Models.Enums;
using StudentsFileSharingApp.Entities.Models.ManyToMany;
using StudentsFileSharingApp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBFile = StudentsFileSharingApp.Entities.Models.File;

namespace StudentsFileSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class GroupsController : BaseController
    {
        private readonly BasicContext context;
        private readonly AppSettings appSettings;

        public GroupsController(BasicContext context, AppSettings appSettings)
        {
            this.context = context;
            this.appSettings = appSettings;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var record = await context.Set<Group>().Include(a => a.Members).FirstOrDefaultAsync(a => a.Id == id);

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
                .ThenInclude(c => c.Author)
                .Include(a => a.Posts)
                .ThenInclude(b => b.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            try
            {
                return Ok(new GroupDto
                {
                    Id = dbResult.Id,
                    Name = dbResult.Name,
                    Posts = dbResult.Posts?.Select(a => new PostDto
                    {
                        Id = a.Id,
                        AuthorName = a.Author.Name,
                        Title = a.Title,
                        IsAuthor = a.AuthorId == userId,
                        Content = a.Content,
                        DateAdded = a.DateAdded,
                        Comments = a.Comments?.Select(b => new PostCommentDto
                        {
                            Id = b.Id,
                            AuthorName = b.Author.Name,
                            IsAuthor = b.AuthorId == userId,
                            Content = b.Content,
                            DateAdded = b.DateAdded
                        }).OrderBy(c => c.DateAdded).ToList()
                    }).OrderByDescending(c => c.DateAdded).ToList()
                });
            }
            catch (Exception e)
            {
                return Ok();
            }
        }

        [HttpPost("[action]/{id}"), ActionName(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            var size = file?.Length;
            var sizeInMB = (size / 1024f) / 1024f;

            if (size <= 0 || sizeInMB > appSettings.MaxFileSizeInMB)
                return BadRequest("Wront file size");

            var fileExtension = MimeTypesMap.GetExtension(file.ContentType);

            var availableFileFormats = new List<string> { "docx", "pdf", "jpeg", "png" };

            if (!availableFileFormats.Any(a => a.Equals(fileExtension)))
                return BadRequest("Wrong file format");

            var userId = GetUserId();

            var groupRecord = await context.Set<Group>().Include(a => a.Members).FirstOrDefaultAsync(a => a.Id == id);
            var userRecord = await context.Set<User>().FindAsync(userId);

            if (groupRecord == null || userRecord == null)
                return NotFound();

            if (!groupRecord.Members.Any(a => a.UserId == userId))
                return BadRequest();

            if (!Directory.Exists(appSettings.FilesPath))
                Directory.CreateDirectory(appSettings.FilesPath);

            var filePath = Path.Combine(appSettings.FilesPath, Path.GetRandomFileName());

            //Gdzieś tu skanowanie antywirusem

            using var stream = System.IO.File.Create(filePath);

            try
            {
                await file.CopyToAsync(stream);

                var dbFile = new DBFile
                {
                    DateAdded = DateTime.UtcNow,
                    DrivePath = filePath,
                    Name = file.FileName,
                    Owner = userRecord,
                    Group = groupRecord,
                    Size = GetFileSizeAsString(size.Value),
                    FileType = fileExtension.Equals(".pdf") || fileExtension.Equals(".docx") ? FileType.Document : FileType.Photo
                };

                context.Set<DBFile>().Add(dbFile);

                await context.SaveChangesAsync();

                return Ok(new FileDto
                {
                    Id = dbFile.Id,
                    Owner = dbFile.Owner.Name,
                    DateAdded = dbFile.DateAdded,
                    FileName = dbFile.Name,
                    IsOwner = true
                });
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        private string GetFileSizeAsString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };

            int order = 0;
            var floatingSize = (double)size;

            while (floatingSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                floatingSize /= 1024;
            }

            return string.Format("{0:0.##} {1}", floatingSize, sizes[order]);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups(string filter)
        {
            var recordsQuery = context.Set<Group>().AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                var loweredFilter = filter.ToLower();

                recordsQuery = recordsQuery.Where(a => a.Name.ToLower().Contains(loweredFilter));
            }

            var records = await recordsQuery.ToListAsync();

            return Ok(records.Select(a => new GroupDto { Id = a.Id, Name = a.Name }));
        }

        [HttpGet("[action]"), ActionName(nameof(GetUserGroups))]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetUserGroups()
        {
            var userId = GetUserId();

            var records = await context.Set<Group>().Where(a => a.Members.Any(b => b.UserId == userId)).ToListAsync();

            return Ok(records.Select(a => new GroupDto { Id = a.Id, Name = a.Name }));
        }

        [HttpPost("[action]/{id}"), ActionName(nameof(JoinGroup))]
        public async Task<ActionResult<GroupDto>> JoinGroup(int id)
        {
            var userId = GetUserId();

            var groupRecord = await context.Set<Group>().Include(a => a.Members).FirstOrDefaultAsync(a => a.Id == id);
            var userRecord = await context.Set<User>().FindAsync(userId);

            if (groupRecord == null || userRecord == null)
                return NotFound();

            if (groupRecord.Members.Any(a => a.UserId == userId))
                return BadRequest();

            groupRecord.Members.Add(new UserGroup
            {
                User = userRecord,
                UserRank = UserRank.Normal
            });

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("[action]/{id}"), ActionName(nameof(LeaveGroup))]
        public async Task<ActionResult<GroupDto>> LeaveGroup(int id)
        {
            var userId = GetUserId();

            var groupRecord = await context.Set<Group>().Include(a => a.Members).FirstOrDefaultAsync(a => a.Id == id);
            var userRecord = await context.Set<User>().FindAsync(userId);

            if (groupRecord == null || userRecord == null)
                return NotFound();

            var member = groupRecord.Members.FirstOrDefault(a => a.UserId == userId);

            if (member == null)
                return BadRequest();

            groupRecord.Members.Remove(member);

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