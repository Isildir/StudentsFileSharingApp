using HeyRed.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Entities.Models.Enums;
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
    public class FilesController : BaseController
    {
        private readonly BasicContext context;
        private readonly AppSettings appSettings;

        public FilesController(BasicContext context, AppSettings appSettings)
        {
            this.context = context;
            this.appSettings = appSettings;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var record = await context.Set<DBFile>().FirstOrDefaultAsync(a => a.Id == id);

            if (record == null)
                return NotFound();

            var userId = GetUserId();

            if (record.OwnerId != userId)
                return BadRequest();

            try
            {
                System.IO.File.Delete(record.DrivePath);

                await context.SaveChangesAsync();

                context.Set<DBFile>().Remove(record);

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                Logger.Log($"{nameof(FilesController)} {nameof(DeleteFile)}", ex.Message, NLog.LogLevel.Error, ex);

                return BadRequest();
            }
            catch (Exception ex)
            {
                Logger.Log($"{nameof(FilesController)} {nameof(DeleteFile)}", ex.Message, NLog.LogLevel.Error, ex);

                return BadRequest();
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
            catch (DbUpdateException ex)
            {
                Logger.Log($"{nameof(FilesController)} {nameof(UploadFile)}", ex.Message, NLog.LogLevel.Error, ex);

                return BadRequest();
            }
            catch (Exception ex)
            {
                Logger.Log($"{nameof(FilesController)} {nameof(UploadFile)}", ex.Message, NLog.LogLevel.Error, ex);

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
    }
}