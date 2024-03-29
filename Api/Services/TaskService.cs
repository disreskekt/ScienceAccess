﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class TaskService
{
    private readonly Context _db;
    private readonly QueueService _queueService;
    private readonly BackgroundServiceManager _backgroundServiceManager;
    private readonly SshService _sshService;

    public TaskService(Context context, QueueService queueService, BackgroundServiceManager backgroundServiceManager, SshService sshService)
    {
        _db = context;
        _queueService = queueService;
        _backgroundServiceManager = backgroundServiceManager;
        _sshService = sshService;
    }

    public async Task StartTask(StartTask startTaskModel)
    {
        Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
            .FirstOrDefaultAsync(ticket => ticket.Id == startTaskModel.TicketId);

        if (ticket is null)
        {
            throw new Exception("Ticket doesn't exist");
        }

        if (!ticket.CanBeUsedRightNow())
        {
            throw new Exception("Ticket can't be used");
        }

        if (ticket.Task.Status is not TaskStatuses.NotStarted)
        {
            throw new Exception("Task has been already started");
        }

        await _db.Entry(ticket.Task).Collection(task => task.FileNames).LoadAsync();

        if (ticket.Task.FileNames is null || !ticket.Task.FileNames.Any())
        {
            throw new Exception("No input files");
        }

        Regex jobExtensionRegex = new Regex(@"^.*\.(job)$");
        bool jobExtensionfound = false;

        foreach (string filename in ticket.Task.FileNames.Select(filename => filename.Name))
        {
            Match match = jobExtensionRegex.Match(filename);
            if (match.Success)
            {
                if (jobExtensionfound)
                {
                    throw new Exception("You can upload only one .job file");
                }

                jobExtensionfound = true;
            }
        }

        if (!jobExtensionfound)
        {
            throw new Exception("No file with .job extension");
        }

        await _queueService.AddToQueue(ticket.Task);

        ticket.Task.Status = TaskStatuses.Pending;

        await _db.SaveChangesAsync();

        _backgroundServiceManager.FastTaskCheck(ticket.Task); //that's right
    }

    public async Task StopTask(Guid taskId, int userId)
    {
        TicketTask task = await _db.Tasks.Include(t => t.Ticket)
                                         .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }

        if (task.Ticket.UserId != userId)
        {
            throw new Exception("Task doesn't belong to you");
        }

        if (task.Status is not TaskStatuses.InProgress or TaskStatuses.Pending)
        {
            throw new Exception("Task not in progress or pending");
        }
        
        _queueService.AddToKillList(task);

        _backgroundServiceManager.FastTaskCheck(task); //that's right
    }

    public async Task StopTaskByAdmin(Guid taskId)
    {
        TicketTask task = await _db.Tasks.FindAsync(taskId);

        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }

        if (task.Status is not TaskStatuses.InProgress or TaskStatuses.Pending)
        {
            throw new Exception("Task not in progress or pending");
        }
        
        _queueService.AddToKillList(task);

        _backgroundServiceManager.FastTaskCheck(task); //that's right
    }
}