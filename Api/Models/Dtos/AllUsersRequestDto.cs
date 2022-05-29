namespace Api.Models.Dtos;

public class AllUsersRequestDto
{
    public bool IsRequested { get; set; }
    public string Comment { get; set; }
    public int? Duration { get; set; }
}