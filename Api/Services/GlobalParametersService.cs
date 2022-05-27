using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Data;
using Api.Options;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace Api.Services;

public class GlobalParametersService
{
    private readonly Context _db;
    private readonly IMapper _mapper;
    
    private static readonly JsonSerializerOptions _jsonWriteOptions;
    public static GlobalParameters GlobalParameters { get; set; }

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
    
    public GlobalParametersService(Context context, IMapper mapper)
    {
        _db = context;
        _mapper = mapper;
    }

    public GlobalParameters GetGlobalParameters()
    {
        return GlobalParameters;
    }
    
    public async Task<GlobalParameters> SetGlobalParameters(GlobalParameters newGlobalParametersModel)
    {
        GlobalParameters = newGlobalParametersModel;
        
        string newJson = JsonSerializer.Serialize(GlobalParameters, _jsonWriteOptions);
        string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        await File.WriteAllTextAsync(appSettingsPath, newJson);

        return GetGlobalParameters();
    }
}