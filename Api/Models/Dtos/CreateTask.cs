using System;
using Microsoft.AspNetCore.Http;

namespace Api.Models.Dtos
{
    public class CreateTask
    {
        public Guid TicketId { get; set; }
        public IFormFileCollection Files { get; set; }
        public string Comment { get; set; }
    }
}