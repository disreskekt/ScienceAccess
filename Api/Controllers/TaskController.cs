using System;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketTask = Api.Models.Task;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly Context _db;

        public TaskController(Context context)
        {
            _db = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> StartTask([FromBody]CreateTask createTaskModel)
        {
            try
            {
                Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
                    .FirstOrDefaultAsync(ticket => ticket.Id == createTaskModel.TicketId);

                if (ticket is null)
                {
                    return BadRequest("Тикет не существует");
                }

                if (!ticket.IsActive)
                {
                    return Forbid("Тикет не может быть использован");
                }
                
                if (ticket.Task is not null)
                {
                    return Forbid("Тикета уже имеет связанную задачу");
                }
                
                ticket.Task = new TicketTask()
                {
                    Comment = createTaskModel.Comment ?? String.Empty,
                    Status = TaskStatuses.NotStarted,
                };
                
                //todo some actions

                await _db.SaveChangesAsync();

                return Ok("Задача создана");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}