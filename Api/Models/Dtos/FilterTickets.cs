using Api.Models.Enums;

namespace Api.Models.Dtos
{
    public class FilterTickets
    {
        public bool? Canceled { get; set; }
        public TicketExpirationStatuses? ExpirationStatus { get; set; }
        public TicketUsageStatuses? UsageStatus { get; set; }
    }
}