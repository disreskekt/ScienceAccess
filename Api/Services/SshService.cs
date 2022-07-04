using System;
using System.Threading.Tasks;
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

    public async Task<string> RunCustomCommand(string command)
    {
        if (!_client.IsConnected)
        {
            _client.Connect();
        }

        Task<string> task = Task.Run(() => _client.RunCommand(command).Result);

        await Task.Delay(250);
        
        return task.Result ?? "";
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