using System;
using System.Threading.Tasks;
using Api.Models.Dtos;

namespace Api.Services.Interfaces;

public interface ITaskService
{
    public Task StartTask(StartTask startTaskModel);

    public Task StopTask(Guid ticketId, int userId);

    public Task StopTaskByAdmin(Guid taskId);
}