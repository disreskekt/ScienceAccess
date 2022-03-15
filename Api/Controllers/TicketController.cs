using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using AutoMapper;
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
                    return BadRequest("Ticket is already requested");
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
                    newTickets[i] = new Ticket()
                    {
                        UserId = user.Id,
                        StartTime = giveTicketsModel.StartTime,
                        EndTime = giveTicketsModel.EndTime,
                        AvailableDuration = giveTicketsModel.Duration,
                        IsCanceled = false,
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

                expressionsArray[1] = filterTicketsModel.ExpirationStatus switch
                {
                    TicketExpirationStatuses.Pending => ticket => ticket.IsPending(),
                    TicketExpirationStatuses.Available => ticket => ticket.IsAvailable(),
                    TicketExpirationStatuses.Expired => ticket => ticket.IsExpired(),
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(filterTicketsModel.UsageStatus))
                };

                expressionsArray[2] = filterTicketsModel.UsageStatus switch
                {
                    TicketUsageStatuses.NotUsed => ticket => ticket.IsNotUsed(),
                    TicketUsageStatuses.InUse => ticket => ticket.IsInUse(),
                    TicketUsageStatuses.Used => ticket => ticket.IsUsed(),
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