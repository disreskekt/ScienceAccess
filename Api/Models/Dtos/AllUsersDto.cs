namespace Api.Models.Dtos
{
    public class AllUsersDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public AllUsersRequestDto TicketRequest { get; set; }
    }
}