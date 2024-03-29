﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Api.Options;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class TicketService
{
    private readonly Context _db;
    private readonly IMapper _mapper;
    private readonly SftpService _sftpService;
    private readonly string _programVersionsFolder;

    public TicketService(Context context, IMapper mapper, SftpService sftpService, IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _db = context;
        _mapper = mapper;
        _sftpService = sftpService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }

    public async Task RequestTicket(int userId, string comment, int? duration)
    {
        if (comment.Length > 300)
        {
            throw new Exception("Too much symbols");
        }
        
        User user = await _db.Users
            .Include(user => user.TicketRequest)
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }
        
        if (user.TicketRequest.IsRequested)
        {
            throw new Exception("Ticket has already been requested");
        }

        user.TicketRequest.IsRequested = true;
        user.TicketRequest.Comment = comment;
        user.TicketRequest.Duration = duration;
        
        //todo maybe some notification
        
        await _db.SaveChangesAsync();
    }

    public async Task CancelRequest(int userId)
    {
        User user = await _db.Users
            .Include(user => user.TicketRequest)
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }
        
        if (!user.TicketRequest.IsRequested)
        {
            throw new Exception("Ticket has not requested");
        }
        
        user.TicketRequest.IsRequested = false;
        
        await _db.SaveChangesAsync();
    }
    
    public async Task<TicketDto[]> GiveTickets(GiveTickets giveTicketsModel)
    {
        if (giveTicketsModel.Count < 1)
        {
            throw new Exception("Can't give less than one ticket");
        }

        if (giveTicketsModel.EndTime <= DateTime.Now)
        {
            throw new Exception("You are trying to give an expired ticket");
        }

        if (giveTicketsModel.EndTime <= giveTicketsModel.StartTime)
        {
            throw new Exception("You are trying to give a ticket with EndTime less than StartTime");
        }

        if (giveTicketsModel.Duration <= 0)
        {
            throw new Exception("Task duration should be more than zero");
        }

        User user = await _db.Users
            .Include(user => user.Tickets)
            .Include(user => user.TicketRequest)
            .FirstOrDefaultAsync(user => user.Id == giveTicketsModel.ReceiverId);

        if (user is null)
        {
            throw new Exception("User doesn't exist");
        }

        string[] listOfFiles = _sftpService.ListOfFiles(_programVersionsFolder);

        if (!listOfFiles.Contains(giveTicketsModel.ProgramVersion))
        {
            throw new Exception("Program version with this name doesn't exist");
        }

        user.TicketRequest.IsRequested = false;

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
                    ProgramVersion = giveTicketsModel.ProgramVersion,
                    Status = TaskStatuses.NotStarted
                }
            };
        }

        user.Tickets.AddRange(newTickets);

        await _db.SaveChangesAsync();

        TicketDto[] ticketDtos = _mapper.Map<TicketDto[]>(newTickets);

        return ticketDtos;
    }

    public async Task ChangeTicket(ChangeTicket changeTicketModel)
    {
        Ticket ticket = await _db.Tickets.FindAsync(changeTicketModel.Id);

        if (ticket is null)
        {
            throw new Exception("Ticket doesn't exist");
        }

        ticket.StartTime = changeTicketModel.StartTime;
        ticket.EndTime = changeTicketModel.EndTime;
        ticket.AvailableDuration = changeTicketModel.AvailableDuration;

        await _db.SaveChangesAsync();
    }

    public async Task CancelTicket(Guid id)
    {
        Ticket ticket = await _db.Tickets.FindAsync(id);

        if (ticket is null)
        {
            throw new Exception("Ticket doesn't exist");
        }

        ticket.IsCanceled = true;

        await _db.SaveChangesAsync();
    }

    public async Task ResumeTicket(Guid id)
    {
        Ticket ticket = await _db.Tickets.FindAsync(id);

        if (ticket is null)
        {
            throw new Exception("Ticket doesn't exist");
        }

        ticket.IsCanceled = false;

        await _db.SaveChangesAsync();
    }
    
    public async Task<TicketDto> GetTicket(Guid ticketId)
    {
        Ticket ticket = await _db.Tickets
            .Include(t => t.Task)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null)
        {
            throw new Exception("Ticket doesn't exist");
        }

        TicketDto ticketDto = _mapper.Map<TicketDto>(ticket);

        return ticketDto;
    }
    
    public async Task<TicketDto> GetMyTicket(Guid ticketId, int userId)
    {
        Ticket ticket = await _db.Tickets
            .Include(t => t.Task)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null || ticket.UserId != userId)
        {
            throw new Exception("You don't have such ticket");
        }

        TicketDto ticketDto = _mapper.Map<TicketDto>(ticket);

        return ticketDto;
    }

    public async Task<List<TicketDto>> GetAll(FilterTickets filterTicketsModel)
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
            TicketExpirationStatuses.Pending => ticket =>
                ticket.StartTime.ToLocalTime() > DateTime.Now,
            TicketExpirationStatuses.Available => ticket =>
                ticket.StartTime.ToLocalTime() <= DateTime.Now && ticket.EndTime.ToLocalTime() > DateTime.Now,
            TicketExpirationStatuses.Expired => ticket => ticket.EndTime.ToLocalTime() <= DateTime.Now,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(filterTicketsModel.UsageStatus))
        };

        expressionsArray[2] = filterTicketsModel.UsageStatus switch
        {
            TicketUsageStatuses.NotUsed => ticket => ticket.Task.Status == TaskStatuses.NotStarted,
            TicketUsageStatuses.InUse => ticket => ticket.Task.Status == TaskStatuses.InProgress ||
                                                   ticket.Task.Status == TaskStatuses.Pending,
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

        dbTickets = filterTicketsModel.PageNumber > 1
            ? dbTickets
                .Skip((filterTicketsModel.PageNumber - 1) * filterTicketsModel.PageSize)
                .Take(filterTicketsModel.PageSize)
            : dbTickets.Take(filterTicketsModel.PageSize);
        
        List<TicketDto> ticketDtos = _mapper.Map<List<TicketDto>>(await dbTickets.ToListAsync());

        return ticketDtos;
    }
}