﻿using System;

namespace Api.Models.Dtos
{
    public class DownloadFiles
    {
        public Guid TaskId { get; set; }
        public string[] Filenames { get; set; }
    }
}