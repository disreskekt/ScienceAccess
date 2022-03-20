using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Dtos;

namespace Api.Services.Interfaces;

public interface ITicketService
{
    public Task RequestTicket(int userId);

    public Task<TicketDto[]> GiveTickets(GiveTickets giveTicketsModel);

    public Task ChangeTicket(ChangeTicket changeTicketModel);

    public Task<TicketDto> GetTicket(Guid ticketId);

    public Task<List<TicketDto>> GetAll(FilterTickets filterTicketsModel);
}