using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Api.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly Context _db;
        private readonly string _userDirectoryPath;
        private readonly LinuxCredentials _linuxCredentials;

        public FileController(Context context, IOptions<LinuxCredentials> linuxCredentials, IOptions<UserFolder> userFolder)
        {
            _db = context;
            _userDirectoryPath = userFolder.Value.Path;
            _linuxCredentials = linuxCredentials.Value;
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesDto uploadFilesModel)
        {
            try
            {
                Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
                                                 .ThenInclude(task => task.FileNames)
                                                 .FirstOrDefaultAsync(ticket => ticket.Id == uploadFilesModel.TaskId);

                if (ticket is null)
                {
                    return BadRequest("Ticket doesn't exist");
                }

                if (!ticket.CanBeUsedRightNow())
                {
                    return BadRequest("Ticket can't be used");
                }
                
                if (ticket.Task.Status is not TaskStatuses.NotStarted)
                {
                    return BadRequest("Task can't use new files");
                }

                if (uploadFilesModel.Files is null || uploadFilesModel.Files.Count < 1)
                {
                    return BadRequest("No input files");
                }

                IEnumerable<string> sendedFiles;
                
                using (SftpHelper sftpClient = new SftpHelper(_linuxCredentials, _userDirectoryPath))
                {
                    if (string.IsNullOrEmpty(ticket.Task.DirectoryPath))
                    {
                        ticket.Task.DirectoryPath = sftpClient.CreateUserFolder(ticket.UserId);
                    }
                    else
                    {
                        sftpClient.CheckUserFolder(ticket.Task.DirectoryPath);
                    }

                    sendedFiles = sftpClient.SendFiles(uploadFilesModel.Files, ticket.Task.DirectoryPath);
                }

                foreach (string filename in sendedFiles)
                {
                    ticket.Task.FileNames.Add(new Filename()
                    {
                        Name = filename,
                        TaskId = ticket.Task.Id,
                        Inputed = true,
                    });
                }

                await _db.SaveChangesAsync();

                return Ok("Files added");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadFiles([FromBody] DownloadFilesDto downloadFilesModel)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token
                
                Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
                    .FirstOrDefaultAsync(ticket => ticket.Id == downloadFilesModel.TaskId);

                if (ticket.UserId != userId)
                {
                    return BadRequest("This is not your task");
                }
                
                if (downloadFilesModel.Filenames is null || downloadFilesModel.Filenames.Length < 1)
                {
                    return BadRequest("Specify files which you want to download");
                }
                
                using (SftpHelper sftpClient = new SftpHelper(_linuxCredentials, _userDirectoryPath))
                {
                    if (downloadFilesModel.Filenames.Length == 1)
                    {
                        string filename = downloadFilesModel.Filenames.First();
                        
                        byte[] file = sftpClient.GetFile(ticket.Task.DirectoryPath, filename);

                        return File(file, "application/octet-stream", filename);
                    }
                    else
                    {
                        byte[] file = sftpClient.GetFiles(ticket.Task.DirectoryPath, downloadFilesModel.Filenames);

                        return File(file, "application/zip", "files.zip");
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}