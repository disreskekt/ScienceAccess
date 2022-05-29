namespace Api.Models.NvidiaSmiModels;

public class NvidiaSmiModel
{
    public Gpu[] Gpus { get; set; }
    public Process[] Processes { get; set; }

    public bool HasFreeGpu => this.Gpus.Length > this.Processes.Length;
}