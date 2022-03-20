using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Dtos;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> RequestTicket()
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                await _ticketService.RequestTicket(userId);

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
                TicketDto[] givenTickets = await _ticketService.GiveTickets(giveTicketsModel);

                return Ok(givenTickets);
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
                await _ticketService.ChangeTicket(changeTicketModel);

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
                TicketDto ticketDto = await _ticketService.GetTicket(ticketId);

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
                List<TicketDto> ticketDtos = await _ticketService.GetAll(filterTicketsModel);

                return Ok(ticketDtos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}