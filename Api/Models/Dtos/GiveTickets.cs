using System;

namespace Api.Models.Dtos
{
    public class GiveTickets
    {
        public int ReceiverId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; } //mins
        public int Count { get; set; }
    }
}