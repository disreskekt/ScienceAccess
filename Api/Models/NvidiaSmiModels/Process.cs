using System;

namespace Api.Models.NvidiaSmiModels;

public class Process : IEquatable<Process>
{
    public int Gpu { get; set; }
    public int Pid { get; set; }
    public string ProcessName { get; set; }

    public bool Equals(Process other)
    {
        return other is not null &&
               this.Gpu == other.Gpu &&
               this.Pid == other.Pid &&
               this.ProcessName == other.ProcessName;
    }
}