using System;
using Microsoft.AspNetCore.Http;

namespace Api.Models.Dtos
{
    public class UploadFilesDto
    {
        public Guid TicketId { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}