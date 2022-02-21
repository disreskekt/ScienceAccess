using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

                User user = await _db.Users.Include(user => user.Tickets).FirstOrDefaultAsync(user => user.Id == userId);

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
                User user = await _db.Users.Include(user => user.Tickets).FirstOrDefaultAsync(user => user.Id == giveTicketModel.ReceiverId);

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
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromBody] GetTickets getTicketsModel)
        {
            try
            {
                //todo refactor this ugly
                Expression<Func<Ticket, bool>>[] expressionsArray = new Expression<Func<Ticket, bool>>[4];

                expressionsArray[0] = getTicketsModel.Active switch
                {
                    true => ticket => ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                                     ((ticket.StartTime <= DateTime.Now &&
                                                                       ticket.EndTime > DateTime.Now) &&
                                                                      (ticket.Task.Status == TaskStatuses.Done ||
                                                                       ticket.Task.Status == TaskStatuses.Failed))),
                    false => ticket => !(ticket.IsCanceled == false && (ticket.StartTime > DateTime.Now ||
                                                                      ((ticket.StartTime <= DateTime.Now &&
                                                                        ticket.EndTime > DateTime.Now) &&
                                                                       (ticket.Task.Status == TaskStatuses.Done ||
                                                                        ticket.Task.Status == TaskStatuses.Failed)))),
                    null => null
                };

                expressionsArray[1] = getTicketsModel.Canceled switch
                {
                    true => ticket => ticket.IsCanceled == true,
                    false => ticket => ticket.IsCanceled == false,
                    null => null
                };

                expressionsArray[2] = getTicketsModel.ExpirationStatus switch
                {
                    TicketExpirationStatuses.Pending => ticket =>
                        ticket.StartTime > DateTime.Now,
                    TicketExpirationStatuses.Available => ticket =>
                        ticket.StartTime <= DateTime.Now && ticket.EndTime > DateTime.Now,
                    TicketExpirationStatuses.Expired => ticket =>
                        ticket.EndTime <= DateTime.Now,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(getTicketsModel.UsageStatus))
                };

                expressionsArray[3] = getTicketsModel.UsageStatus switch
                {
                    TicketUsageStatuses.NotUsed => ticket => ticket.Task == null ||
                                                             ticket.Task.Status == TaskStatuses.NotStarted,
                    TicketUsageStatuses.InUse => ticket => ticket.Task.Status == TaskStatuses.InProgress,
                    TicketUsageStatuses.Used => ticket => ticket.Task.Status == TaskStatuses.Done ||
                                                          ticket.Task.Status == TaskStatuses.Failed,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(getTicketsModel.UsageStatus))
                };

                IQueryable<Ticket> dbTickets = _db.Tickets.Include(ticket => ticket.Task).AsQueryable();

                foreach (Expression<Func<Ticket, bool>> expression in expressionsArray)
                {
                    if (expression is null)
                    {
                        continue;
                    }

                    dbTickets = dbTickets.Where(expression);
                }

                return Ok(await dbTickets.ToListAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}