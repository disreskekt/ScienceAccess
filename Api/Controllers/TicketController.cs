using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketTask = Api.Models.Task;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly Context _db;
        private readonly IMapper _mapper;

        public TicketController(Context context, IMapper mapper)
        {
            _db = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> RequestTicket()
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                User user = await _db.Users.Include(user => user.Tickets).FirstOrDefaultAsync(user => user.Id == userId);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }

                bool hasTicketRequest = user.TicketRequest;

                if (hasTicketRequest)
                {
                    return BadRequest("Ticket has already been requested");
                }

                user.TicketRequest = true;
                //todo maybe some notification
                await _db.SaveChangesAsync();

                return Ok("Ticket is requested");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GiveTickets([FromBody] GiveTickets giveTicketsModel)
        {
            try
            {
                if (giveTicketsModel.Count < 1)
                {
                    return BadRequest("Can't give less than one ticket");
                }
                
                if (giveTicketsModel.EndTime <= DateTime.Now)
                {
                    return BadRequest("You are trying to give an expired ticket");
                }

                if (giveTicketsModel.Duration <= 0)
                {
                    return BadRequest("Task duration should be more than zero");
                }
                
                User user = await _db.Users.Include(user => user.Tickets)
                    .FirstOrDefaultAsync(user => user.Id == giveTicketsModel.ReceiverId);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }

                user.TicketRequest = false;

                Ticket[] newTickets = new Ticket[giveTicketsModel.Count];
                for (int i = 0; i < giveTicketsModel.Count; i++)
                {
                    newTickets[i] = new Ticket
                    {
                        UserId = user.Id,
                        StartTime = giveTicketsModel.StartTime,
                        EndTime = giveTicketsModel.EndTime,
                        AvailableDuration = giveTicketsModel.Duration,
                        IsCanceled = false,
                        Task = new TicketTask
                        {
                            Status = TaskStatuses.NotStarted
                        }
                    };
                }
                
                user.Tickets.AddRange(newTickets);
                
                await _db.SaveChangesAsync();

                return Ok("Ticket issued");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeTicket([FromBody] ChangeTicket changeTicketModel)
        {
            try
            {
                Ticket ticket = await _db.Tickets.FindAsync(changeTicketModel.Id);
                
                if (ticket is null)
                {
                    return BadRequest("Ticket doesn't exist");
                }

                ticket.StartTime = changeTicketModel.StartTime ?? ticket.StartTime;
                ticket.EndTime = changeTicketModel.EndTime ?? ticket.EndTime;
                ticket.AvailableDuration = changeTicketModel.AvailableDuration ?? ticket.AvailableDuration;
                ticket.IsCanceled = changeTicketModel.IsCanceled ?? ticket.IsCanceled;

                await _db.SaveChangesAsync();

                return Ok("Ticket changed");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTicket([FromQuery] Guid ticketId)
        {
            try
            {
                Ticket ticket = await _db.Tickets
                    .Include(t => t.Task)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket is null)
                {
                    return BadRequest("Ticket doesn't exist");
                }

                TicketDto ticketDto = _mapper.Map<TicketDto>(ticket);

                return Ok(ticketDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromBody] FilterTickets filterTicketsModel)
        {
            try
            {
                Expression<Func<Ticket, bool>>[] expressionsArray = new Expression<Func<Ticket, bool>>[3];

                expressionsArray[0] = filterTicketsModel.Canceled switch
                {
                    true => ticket => ticket.IsCanceled == true,
                    false => ticket => ticket.IsCanceled == false,
                    null => null
                };
                //I can't use methods in expressions :(
                expressionsArray[1] = filterTicketsModel.ExpirationStatus switch
                {
                    TicketExpirationStatuses.Pending => ticket => ticket.StartTime <= DateTime.Now && ticket.EndTime > DateTime.Now,
                    TicketExpirationStatuses.Available => ticket => ticket.StartTime <= DateTime.Now && ticket.EndTime > DateTime.Now,
                    TicketExpirationStatuses.Expired => ticket => ticket.EndTime <= DateTime.Now,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(filterTicketsModel.UsageStatus))
                };

                expressionsArray[2] = filterTicketsModel.UsageStatus switch
                {
                    TicketUsageStatuses.NotUsed => ticket => ticket.Task.Status == TaskStatuses.NotStarted,
                    TicketUsageStatuses.InUse => ticket => ticket.Task.Status == TaskStatuses.InProgress,
                    TicketUsageStatuses.Used => ticket => ticket.Task.Status == TaskStatuses.Done ||
                                                          ticket.Task.Status == TaskStatuses.Failed,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(filterTicketsModel.UsageStatus))
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

                List<TicketDto> ticketDtos = _mapper.Map<List<TicketDto>>(await dbTickets.ToListAsync());
                
                return Ok(ticketDtos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}