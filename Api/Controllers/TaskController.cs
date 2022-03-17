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
using TicketTask = Api.Models.Task;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly Context _db;
        private readonly string _userDirectoryPath;
        private readonly LinuxCredentials _linuxCredentials;

        public TaskController(Context context, IOptions<LinuxCredentials> linuxCredentials, IOptions<UserFolder> userFolder)
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
                    .FirstOrDefaultAsync(ticket => ticket.Id == uploadFilesModel.TicketId);

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

                Regex jobExtensionRegex = new Regex(@"^.*\.(job)$");
                bool jobExtensionfound = false;
                
                foreach (string filename in uploadFilesModel.Files.Select(file => file.FileName).Concat(ticket.Task.FileNames.Select(filename => filename.Name)))
                {
                    Match match = jobExtensionRegex.Match(filename);
                    if (match.Success)
                    {
                        jobExtensionfound = true;
                        break;
                    }
                }

                if (!jobExtensionfound)
                {
                    return BadRequest("No file with .job extension");
                }

                string currentDirectory;
                IEnumerable<string> sendedFiles;
                
                using (SftpHelper sftpClient = new SftpHelper(_linuxCredentials, _userDirectoryPath))
                {
                    currentDirectory = sftpClient.CreateUserFolder(ticket.UserId);
                    sendedFiles = sftpClient.SendFiles(uploadFilesModel.Files, currentDirectory);
                }

                foreach (string filename in sendedFiles)
                {
                    ticket.Task.FileNames.Add(new Filename()
                    {
                        Name = filename,
                        TaskId = ticket.Task.Id
                    });
                }

                return Ok("Files added");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> StartTask([FromForm]StartTask startTaskModel)
        {
            try
            {
                Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
                    .FirstOrDefaultAsync(ticket => ticket.Id == startTaskModel.TicketId);

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
                    return BadRequest("Task has been already started");
                }
                
                await _db.Entry(ticket.Task).Collection(task => task.FileNames).LoadAsync();

                if (ticket.Task.FileNames is null || ticket.Task.FileNames.Count < 1)
                {
                    return BadRequest("No input files");
                }

                Regex jobExtensionRegex = new Regex(@"^.*\.(job)$");
                bool jobExtensionfound = false;
                
                foreach (string filename in ticket.Task.FileNames.Select(filename => filename.Name))
                {
                    Match match = jobExtensionRegex.Match(filename);
                    if (match.Success)
                    {
                        jobExtensionfound = true;
                        break;
                    }
                }

                if (!jobExtensionfound)
                {
                    return BadRequest("No file with .job extension");
                }

                //todo start computing

                //todo some actions

                await _db.SaveChangesAsync();

                return Ok("Task started");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> StopTask([FromQuery] Guid ticketId)
        {
            try
            {
                Ticket ticket = await _db.Tickets.Include(t => t.Task)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                if (ticket.UserId != userId)
                {
                    return BadRequest("Task doesn't belong to you");
                }
                
                if (ticket.Task is null)
                {
                    return BadRequest("Task doesn't exist");
                }

                if (ticket.Task.Status is not TaskStatuses.InProgress)
                {
                    return BadRequest("Task not in progress");
                }
                
                //todo some actions

                return Ok("Task stopped");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StopTaskByAdmin([FromQuery] Guid taskId)
        {
            try
            {
                TicketTask task = await _db.Tasks.FindAsync(taskId);

                if (task is null)
                {
                    return BadRequest("Task doesn't exist");
                }

                if (task.Status is not TaskStatuses.InProgress)
                {
                    return BadRequest("Task not in progress");
                }
                
                //todo some actions

                return Ok("Task stopped");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}