﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Exceptions;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
using Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class FileService
{
    private readonly Context _db;
    private readonly string _baseFolderPath;
    private readonly LinuxCredentials _linuxCredentials;

    public FileService(Context context, IOptions<LinuxCredentials> linuxCredentials, IOptions<BaseFolder> baseFolder)
    {
        _db = context;
        _baseFolderPath = baseFolder.Value.Path;
        _linuxCredentials = linuxCredentials.Value;
    }

    public async Task UploadFiles(UploadFilesDto uploadFilesModel)
    {
        TicketTask task = await _db.Tasks
            .Include(task => task.FileNames)
            .Include(task => task.Ticket)
            .ThenInclude(ticket => ticket.User)
            .FirstOrDefaultAsync(task => task.Id == uploadFilesModel.TaskId);

        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }

        if (!task.Ticket.CanBeUsedRightNow())
        {
            throw new Exception("Ticket can't be used");
        }

        if (task.Status is not TaskStatuses.NotStarted)
        {
            throw new Exception("Task can't use new files");
        }

        if (uploadFilesModel.Files is null || !uploadFilesModel.Files.Any())
        {
            throw new Exception("No input files");
        }

        IEnumerable<string> sendedFiles;

        using (SftpService sftpClient = new SftpService(_linuxCredentials, _baseFolderPath))
        {
            if (string.IsNullOrEmpty(task.DirectoryPath))
            {
                task.DirectoryPath = sftpClient.CreateUserFolder(task.Ticket.User.Email);
            }
            else
            {
                sftpClient.CheckUserFolder(task.DirectoryPath);
            }

            string taskDirectory = task.DirectoryPath + "/" + task.Id;
            
            sendedFiles = sftpClient.SendFiles(uploadFilesModel.Files, taskDirectory);
        }

        foreach (string filename in sendedFiles)
        {
            if (task.FileNames.Select(fname => fname.Name).Contains(filename))
            {
                continue;
            }
            
            task.FileNames.Add(new Filename()
            {
                Name = filename,
                TaskId = task.Id,
                Inputed = true,
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<string[]> GetFiles(int userId, Guid taskId, bool? isInputed = null)
    {
        TicketTask task = await _db.Tasks.Include(task => task.Ticket)
                                         .Include(task => task.FileNames)
                                         .FirstOrDefaultAsync(task => task.Id == taskId);
        
        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }
        
        if (task.Ticket.UserId != userId)
        {
            throw new Exception("This is not your task");
        }

        string[] filenames = task.FileNames
            .Where(filename => isInputed is not null ? filename.Inputed == isInputed : true)
            .Select(filename => filename.Name)
            .ToArray();

        return filenames;
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

        if (downloadFilesModel.Filenames is null || !downloadFilesModel.Filenames.Any())
        {
            throw new Exception("Specify files which you want to download");
        }
        
        string taskDirectory = task.DirectoryPath + "/" + task.Id;
        
        using (SftpService sftpClient = new SftpService(_linuxCredentials, _baseFolderPath))
        {
            if (downloadFilesModel.Filenames.Length == 1)
            {
                string filename = downloadFilesModel.Filenames.First();

                return sftpClient.GetFile(taskDirectory, filename);
            }
            else
            {
                return sftpClient.GetFiles(taskDirectory, downloadFilesModel.Filenames);
            }
        }
    }

    public async Task DeleteFiles(DeleteFilesDto deleteFilesModel, int userId)
    {
        TicketTask task = await _db.Tasks.Include(task => task.FileNames)
                                         .Include(task => task.Ticket)
                                         .FirstOrDefaultAsync(task => task.Id == deleteFilesModel.TaskId);
        
        if (task is null)
        {
            throw new Exception("Task doesn't exist");
        }
        
        if (task.Ticket.UserId != userId)
        {
            throw new Exception("This is not your task");
        }

        if (deleteFilesModel.Filenames is null || !deleteFilesModel.Filenames.Any())
        {
            throw new Exception("Specify files which you want to delete");
        }
        
        if (string.IsNullOrWhiteSpace(task.DirectoryPath))
        {
            throw new Exception("Nothing to delete");
        }
        
        string taskDirectory = task.DirectoryPath + "/" + task.Id;
        
        using (SftpService sftpClient = new SftpService(_linuxCredentials, _baseFolderPath))
        {
            foreach (string filename in deleteFilesModel.Filenames)
            {
                sftpClient.DeleteFile(taskDirectory, filename);

                Filename filenameToDelete = task.FileNames.Single(fname => fname.Name == filename);

                _db.Remove(filenameToDelete);
            }

            await _db.SaveChangesAsync();
        }
    }
}