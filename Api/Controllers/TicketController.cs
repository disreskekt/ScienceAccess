using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly Context _db;

        public TicketController(Context context)
        {
            _db = context;
        }

        [HttpPost]
        public async Task<IActionResult> RequestTicket()
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                User user = _db.Users.Include(user => user.Tickets).FirstOrDefault(user => user.Id == userId);

                if (user is null)
                {
                    return BadRequest("Пользователь не существует");
                }

                bool hasTicketRequest = user.TicketRequest;

                if (hasTicketRequest)
                {
                    return Forbid("Тикет уже запрошен");
                }

                bool hasActiveTicket = user.Tickets.Any(ticket => ticket.IsActive);

                if (hasActiveTicket)
                {
                    return Forbid("Уже есть активный тикет");
                }

                user.TicketRequest = true;
                //todo maybe some notification
                await _db.SaveChangesAsync();

                return Ok("Тикет запрошен");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GiveTicket([FromBody] GiveTicket giveTicketModel)
        {
            try
            {
                User user = _db.Users.Include(user => user.Tickets).FirstOrDefault(user => user.Id == giveTicketModel.ReceiverId);

                if (user is null)
                {
                    return BadRequest("Пользователь не существует");
                }

                if (user.Tickets.Any(ticket => ticket.IsActive))
                {
                    return Forbid("Пользователь имеет активный тикет");
                }

                if (giveTicketModel.EndTime <= DateTime.Now)
                {
                    return BadRequest("Вы пытаетесь выдать просроченный тикет");
                }

                user.TicketRequest = false;
                user.Tickets.Add(
                    new Ticket()
                    {
                        UserId = user.Id,
                        StartTime = giveTicketModel.StartTime,
                        EndTime = giveTicketModel.EndTime,
                        AvailableDuration = giveTicketModel.Duration,
                        ExpirationStatus = giveTicketModel.StartTime >= DateTime.Now
                            ? TicketExpirationStatuses.Available
                            : TicketExpirationStatuses.Pending,
                        UsageStatus = TicketUsageStatuses.NotUsed,
                        IsCanceled = false,
                    }
                );
                
                await _db.SaveChangesAsync();

                return Ok("Тикет выдан");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}