using System;

namespace Api.Models.Dtos
{
    public class DownloadFilesDto
    {
        public Guid TaskId { get; set; }
        public string[] Filenames { get; set; }
    }
}