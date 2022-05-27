using System;

namespace Api.Models.Dtos;

public class DeleteFiles
{
    public Guid TaskId { get; set; }
    public string[] Filenames { get; set; }
}