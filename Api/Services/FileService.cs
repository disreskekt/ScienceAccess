using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Api.Options;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class FileService : IFileService
{
    private readonly Context _db;
    private readonly string _userDirectoryPath;
    private readonly LinuxCredentials _linuxCredentials;

    public FileService(Context context, IOptions<LinuxCredentials> linuxCredentials, IOptions<UserFolder> userFolder)
    {
        _db = context;
        _userDirectoryPath = userFolder.Value.Path;
        _linuxCredentials = linuxCredentials.Value;
    }

    public async Task UploadFiles(UploadFilesDto uploadFilesModel)
    {
        Ticket ticket = await _db.Tickets.Include(ticket => ticket.Task)
            .ThenInclude(task => task.FileNames)
            .FirstOrDefaultAsync(ticket => ticket.Id == uploadFilesModel.TaskId);

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
            throw new Exception("Task can't use new files");
        }

        if (uploadFilesModel.Files is null || uploadFilesModel.Files.Count < 1)
        {
            throw new Exception("No input files");
        }

        IEnumerable<string> sendedFiles;

        using (SftpService sftpClient = new SftpService(_linuxCredentials, _userDirectoryPath))
        {
            if (string.IsNullOrEmpty(ticket.Task.DirectoryPath))
            {
                ticket.Task.DirectoryPath = sftpClient.CreateUserFolder(ticket.UserId);
            }
            else
            {
                sftpClient.CheckUserFolder(ticket.Task.DirectoryPath);
            }

            sendedFiles = sftpClient.SendFiles(uploadFilesModel.Files, ticket.Task.DirectoryPath);
        }

        foreach (string filename in sendedFiles)
        {
            ticket.Task.FileNames.Add(new Filename()
            {
                Name = filename,
                TaskId = ticket.Task.Id,
                Inputed = true,
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<byte[]> DownloadFiles(DownloadFilesDto downloadFilesModel, int userId)
    {
        TicketTask task = await _db.Tasks.Include(task => task.Ticket)
                                         .FirstOrDefaultAsync(task => task.Id == downloadFilesModel.TaskId);

        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }
        
        if (task.Ticket.UserId != userId)
        {
            throw new Exception("This is not your task");
        }

        if (downloadFilesModel.Filenames is null || downloadFilesModel.Filenames.Length < 1)
        {
            throw new Exception("Specify files which you want to download");
        }

        using (SftpService sftpClient = new SftpService(_linuxCredentials, _userDirectoryPath))
        {
            if (downloadFilesModel.Filenames.Length == 1)
            {
                string filename = downloadFilesModel.Filenames.First();

                return sftpClient.GetFile(task.DirectoryPath, filename);
            }
            else
            {
                return sftpClient.GetFiles(task.DirectoryPath, downloadFilesModel.Filenames);
            }
        }
    }
}