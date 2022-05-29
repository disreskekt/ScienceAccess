namespace Api.Models.Dtos;

public class UserTicketRequestDto
{
    public bool IsRequested { get; set; }
    public string Comment { get; set; }
    public int? Duration { get; set; }
}