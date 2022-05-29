namespace Api.Models.NvidiaSmiModels;

public class Process
{
    public int Gpu { get; set; }
    public int Pid { get; set; }
    public string ProcessName { get; set; }
}