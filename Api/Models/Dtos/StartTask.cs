using System;

namespace Api.Models.Dtos
{
    public class StartTask
    {
        public Guid TicketId { get; set; }
        public string Comment { get; set; }
    }
}