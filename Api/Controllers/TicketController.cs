using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<IActionResult> RequestTicket([FromBody] TicketRequestDto ticketRequestDto)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                await _ticketService.RequestTicket(userId, ticketRequestDto.Comment, ticketRequestDto.Duration);

                return Ok("Ticket is requested");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest()
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                await _ticketService.CancelRequest(userId);

                return Ok("Ticket request cancelled");
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
        public async Task<IActionResult> CancelTicket([FromQuery] Guid id)
        {
            try
            {
                await _ticketService.CancelTicket(id);

                return Ok("Ticket canceled");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResumeTicket([FromQuery] Guid id)
        {
            try
            {
                await _ticketService.ResumeTicket(id);

                return Ok("Ticket resumed");
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
        
        [HttpGet]
        public async Task<IActionResult> GetMyTicket([FromQuery] Guid ticketId)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token
                
                TicketDto ticketDto = await _ticketService.GetMyTicket(ticketId, userId);

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