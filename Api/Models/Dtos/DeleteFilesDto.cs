using System;

namespace Api.Models.Dtos;

public class DeleteFilesDto
{
    public Guid TaskId { get; set; }
    public string[] Filenames { get; set; }
}