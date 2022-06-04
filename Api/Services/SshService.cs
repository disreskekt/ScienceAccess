using System;
using Api.Options;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Api.Services;

public class SshService : IDisposable
{
    private readonly SshClient _client;

    public SshService(IOptions<LinuxCredentials> linuxCredentialOptions)
    {
        LinuxCredentials linuxCredentials = linuxCredentialOptions.Value;
        
        _client = new SshClient(linuxCredentials.Host,
            linuxCredentials.Port,
            linuxCredentials.Username,
            linuxCredentials.Password);
    }

    public string GetStatus()
    {
        if (!_client.IsConnected)
        {
            _client.Connect();
        }

        SshCommand command = _client.RunCommand("nvidia-smi");

        return command.Result;
    }

    public void RunTask(string directory, string programPath, string jobFileName, int gpu, int streams)
    {
        if (!_client.IsConnected)
        {
            _client.Connect();
        }

        string cdCommand = $"cd {directory}";
        string mainCommand = $"{programPath} -cfg {jobFileName} -gpu {gpu} -streams {streams} >out.txt 2>&1 \\&";
        string disownCommand = "disown -r";

        _client.RunCommand($"{cdCommand} && {mainCommand} && {disownCommand}");
    }

    public string RunCustomCommand(string command)
    {
        if (!_client.IsConnected)
        {
            _client.Connect();
        }

        return _client.RunCommand(command).Result;
    }
    
    public void Dispose()
    {
        if (_client.IsConnected)
        {
            _client.Disconnect();
        }
        
        _client.Dispose();
    }
}