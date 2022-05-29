using System;
using System.Collections.Generic;
using System.Linq;
using Api.Models.NvidiaSmiModels;

namespace Api.Helpers;

public static class NvidiaSmiParser
{
    private const string ARRAY_START = "=|\n|";
    private const string GPU_ARRAY_CONTINUE_START = "|\n+-";
    private const string GPU_ARRAY_CONTINUE_END = "-+\n|";
    private const string PROCESS_ARRAY_CONTINUE_START = "|\n";
    private const string PROCESS_ARRAY_CONTINUE_END = "\n|";
    private const string TABLE_START = " \n+-";
    private const string TABLE_END = "-+\n ";
    
    public static NvidiaSmiModel ParseNvidiaSmiResult(string nvidiaSmiResult)
    {
        int indexOfEmptyRowStart = nvidiaSmiResult.IndexOf(TABLE_END, StringComparison.Ordinal) + 3;
        int indexOfEmptyRowEnd = nvidiaSmiResult.IndexOf(TABLE_START, indexOfEmptyRowStart, StringComparison.Ordinal);

        if (indexOfEmptyRowStart < 1 || indexOfEmptyRowEnd < 1)
        {
            throw new Exception("Something wrong with NvidiaSmiResult");
        }

        string gpuString = nvidiaSmiResult.Substring(0, indexOfEmptyRowStart);

        string processesString = nvidiaSmiResult.Substring(indexOfEmptyRowEnd);

        IEnumerable<Gpu> gpus = ParseGpus(gpuString);
        
        Process[] processes = ParseProcesses(processesString);

        return new NvidiaSmiModel()
        {
            Gpus = gpus.ToArray(),
            Processes = processes
        };
    }

    private static IEnumerable<Gpu> ParseGpus(string gpuString)
    {
        int indexOfArrayStart = gpuString.IndexOf(ARRAY_START, StringComparison.Ordinal);

        if (indexOfArrayStart < 1)
        {
            throw new Exception("Something wrong with NvidiaSmiResult");
        }

        int gpuRowFirstCharIndex = indexOfArrayStart + 3;

        while (true)
        {
            Gpu gpu = new Gpu();

            int indexOfArrayContinueStart = gpuString.IndexOf(GPU_ARRAY_CONTINUE_START, gpuRowFirstCharIndex, StringComparison.Ordinal);

            int indexOfSecondSeparator = gpuString.IndexOf('|', gpuRowFirstCharIndex + 1);

            gpu.Id = int.Parse(gpuString
                .Substring(gpuRowFirstCharIndex + 1, indexOfSecondSeparator - gpuRowFirstCharIndex - 1)
                .Split(' ')
                .First(str => str.Length > 0));

            int indexOfPreLastSeparator = gpuString.LastIndexOf('|', indexOfArrayContinueStart - 1, indexOfArrayContinueStart - gpuRowFirstCharIndex + 1);

            gpu.GpuUtil = int.Parse(gpuString
                .Substring(indexOfPreLastSeparator + 1, indexOfArrayContinueStart - indexOfPreLastSeparator - 1)
                .Split(' ')
                .First(str => str.Length > 0)
                .TrimEnd('%'));
            
            yield return gpu;
            
            int indexOfArrayContinueEnd = gpuString.IndexOf(GPU_ARRAY_CONTINUE_END, indexOfArrayContinueStart, StringComparison.Ordinal);

            if (indexOfArrayContinueEnd > 1)
            {
                gpuRowFirstCharIndex = indexOfArrayContinueEnd + 3;
            }
            else
            {
                break;
            }
        }
    }

    private static Process[] ParseProcesses(string processesString)
    {
        int indexOfArrayStart = processesString.IndexOf(ARRAY_START, StringComparison.Ordinal);

        if (indexOfArrayStart < 1)
        {
            throw new Exception("Something wrong with NvidiaSmiResult");
        }

        int processRowFirstCharIndex = indexOfArrayStart + 3;

        List<Process> processesToReturn = new List<Process>();

        while (true)
        {
            Process process = new Process();

            int indexOfArrayContinueStart = processesString.IndexOf(PROCESS_ARRAY_CONTINUE_START, processRowFirstCharIndex, StringComparison.Ordinal);

            string[] columns = processesString.Substring(processRowFirstCharIndex + 1, indexOfArrayContinueStart - processRowFirstCharIndex - 1).Split(' ').Where(str => str.Length > 0).ToArray();

            process.Gpu = int.Parse(columns.First());
            
            process.Pid = int.Parse(columns.Skip(1).First());

            process.ProcessName = columns.Skip(3).First();

            if (!process.ProcessName.Contains("Xorg"))
            {
                processesToReturn.Add(process);
            }

            int indexOfArrayContinueEnd = processesString.IndexOf(PROCESS_ARRAY_CONTINUE_END, indexOfArrayContinueStart, StringComparison.Ordinal);

            if (indexOfArrayContinueEnd > 1)
            {
                processRowFirstCharIndex = indexOfArrayContinueEnd + 1;
            }
            else
            {
                break;
            }
        }

        HashSet<int> hashSet = new HashSet<int>();

        foreach (Process process in processesToReturn.OrderByDescending(pr => pr.Gpu))
        {
            if (!hashSet.Add(process.Pid))
            {
                processesToReturn.Remove(process);
            }
        }

        return processesToReturn.ToArray();
    }
}