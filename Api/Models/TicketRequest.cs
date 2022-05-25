using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class TicketRequest
{
    [Key]
    [ForeignKey("User")]
    public int Id { get; set; }
    public bool IsRequested { get; set; }
    public string Comment { get; set; }
    public int? Duration { get; set; }
}