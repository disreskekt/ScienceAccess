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
        public async Task<IActionResult> StartTask([FromForm]CreateTask createTaskModel)
        {
            try
            {
                Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
                    .FirstOrDefaultAsync(ticket => ticket.Id == createTaskModel.TicketId);

                if (ticket is null)
                {
                    return BadRequest("Ticket doesn't exist");
                }

                if (!ticket.CanBeUsedRightNow())
                {
                    return BadRequest("Ticket can't be used");
                }
                
                if (ticket.Task is not null)
                {
                    return BadRequest("Ticket is already assigned to the task");
                }

                if (createTaskModel.Files is null || createTaskModel.Files.Count < 1)
                {
                    return BadRequest("No input files");
                }

                Regex jobExtensionRegex = new Regex(@"^.*\.(job)$");
                bool jobExtensionfound = false;
                
                foreach (string filename in createTaskModel.Files.Select(file => file.FileName))
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
                
                await _db.Entry(ticket).Reference(tic => tic.User).LoadAsync();

                string currentDirectory;
                IEnumerable<string> sendedFiles;
                
                using (SftpHelper sftpClient = new SftpHelper(_linuxCredentials, _userDirectoryPath))
                {
                    currentDirectory = sftpClient.CreateUserFolder(ticket.User.Lastname + ticket.User.Name);
                    sendedFiles = sftpClient.SendFiles(createTaskModel.Files, currentDirectory);
                }
                
                //todo start computing
                
                ticket.Task = new TicketTask()
                {
                    Comment = createTaskModel.Comment ?? String.Empty,
                    Status = TaskStatuses.NotStarted,
                    DirectoryPath = currentDirectory,
                    FileNames = sendedFiles.ToArray()
                };
                
                //todo some actions

                await _db.SaveChangesAsync();

                return Ok("Task started");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}