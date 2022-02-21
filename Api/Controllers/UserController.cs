using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly Context _db; 
        
        public UserController(Context context)
        {
            _db = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<UserDto> usersList = await _db.Users.Select(user => new UserDto()
                { //todo refactor this ugly too its just IsActive == true
                    Id = user.Id,
                    FullName = user.Name + " " + user.Lastname,
                    TicketRequest = user.TicketRequest,
                    ActiveTicket = user.Tickets.FirstOrDefault(ticket =>
                        ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                       ((ticket.StartTime <= DateTime.Now &&
                                                         ticket.EndTime > DateTime.Now) &&
                                                        (ticket.Task.Status == TaskStatuses.Done ||
                                                         ticket.Task.Status == TaskStatuses.Failed)))) != null
                        ? user.Tickets.FirstOrDefault(ticket =>
                            ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                           ((ticket.StartTime <= DateTime.Now &&
                                                             ticket.EndTime > DateTime.Now) &&
                                                            (ticket.Task.Status == TaskStatuses.Done ||
                                                             ticket.Task.Status == TaskStatuses.Failed)))).Id
                        : null,
                    TaskStatus = user.Tickets.FirstOrDefault(ticket =>
                        ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                       ((ticket.StartTime <= DateTime.Now &&
                                                         ticket.EndTime > DateTime.Now) &&
                                                        (ticket.Task.Status == TaskStatuses.Done ||
                                                         ticket.Task.Status == TaskStatuses.Failed)))) != null
                        ? user.Tickets.FirstOrDefault(ticket =>
                            ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                           ((ticket.StartTime <= DateTime.Now &&
                                                             ticket.EndTime > DateTime.Now) &&
                                                            (ticket.Task.Status == TaskStatuses.Done ||
                                                             ticket.Task.Status == TaskStatuses.Failed)))).Task.Status
                        : null,
                }).ToListAsync();

                return Ok(usersList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}