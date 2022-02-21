using Api.Models.Enums;

namespace Api.Models.Dtos
{
    public class GetTickets
    {
        public bool? Active { get; set; }
        public bool? Canceled { get; set; }
        public TicketExpirationStatuses? ExpirationStatus { get; set; }
        public TicketUsageStatuses? UsageStatus { get; set; }
    }
}