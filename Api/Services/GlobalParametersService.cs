using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class GlobalParametersService
{
    private static readonly JsonSerializerOptions _jsonWriteOptions;
    public static GlobalParameters GlobalParameters { get; set; }
    
    private readonly SftpService _sftpService;
    private readonly string _programVersionsFolder;

    static GlobalParametersService()
    {
        GlobalParameters = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection("GlobalParameters")
            .Get<GlobalParameters>();
        
        _jsonWriteOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
    }

    public GlobalParametersService(SftpService sftpService, IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _sftpService = sftpService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }
    
    public GlobalParameters GetGlobalParameters()
    {
        return GlobalParameters ?? new GlobalParameters() {GlobalParametersDictionary = new Dictionary<string, string>()};
    }
    
    public async Task<GlobalParameters> SetGlobalParameters(GlobalParameters newGlobalParametersModel)
    {
        GlobalParameters = newGlobalParametersModel;
        
        string newJson = JsonSerializer.Serialize(GlobalParameters, _jsonWriteOptions);
        string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        await File.WriteAllTextAsync(appSettingsPath, newJson);

        return GetGlobalParameters();
    }

    public string[] GetAvailableProgramVersions()
    {
        return _sftpService.ListOfFiles(_programVersionsFolder);
    }
}