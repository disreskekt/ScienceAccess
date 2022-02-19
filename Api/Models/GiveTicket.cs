using System;

namespace Api.Models
{
    public class GiveTicket
    {
        public int ReceiverId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; } //mins
    }
}