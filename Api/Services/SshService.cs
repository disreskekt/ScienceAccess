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