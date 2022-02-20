using System;

namespace Api.Models.Dtos
{
    public class CreateTask
    {
        public Guid TicketId { get; set; }
        public string Comment { get; set; }
    }
}