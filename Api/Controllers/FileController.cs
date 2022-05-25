﻿using System;
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
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesDto uploadFilesModel)
        {
            try
            {
                await _fileService.UploadFiles(uploadFilesModel);

                return Ok("Files added");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableFiles([FromQuery] Guid taskId, [FromQuery] bool? isInputed = null)
        {
            try
            {
                int userId = GetCurrentUserId();

                return Ok(await _fileService.GetFiles(userId, taskId, isInputed));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DownloadFiles([FromBody] DownloadFilesDto downloadFilesModel)
        {
            try
            {
                int userId = GetCurrentUserId();

                byte[] file = await _fileService.DownloadFiles(downloadFilesModel, userId);

                if (downloadFilesModel.Filenames.Length == 1)
                {
                    string filename = downloadFilesModel.Filenames.First();
                    
                    return File(file, "application/octet-stream", filename);
                }
                else
                {
                    return File(file, "application/zip", "files.zip");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] Guid taskId, [FromQuery] string filename)
        {
            try
            {
                int userId = GetCurrentUserId();

                await _fileService.DeleteFile(userId, taskId, filename);
                
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private int GetCurrentUserId()
        {
            int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

            return userId;
        }
    }
}