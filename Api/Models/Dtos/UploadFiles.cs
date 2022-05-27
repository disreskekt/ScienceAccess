using System;
using Microsoft.AspNetCore.Http;

namespace Api.Models.Dtos
{
    public class UploadFiles
    {
        public Guid TaskId { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}