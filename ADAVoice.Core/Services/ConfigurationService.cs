using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace ADAVoice.Core.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public class ConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private const string ConfigFileName = "appsettings.json";
    private const string EnvFileName = ".env";
    private readonly string _basePath;
    
    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        // Get the base path - look for .env file in multiple locations
        _basePath = FindBasePath();
    }
    
    private string FindBasePath()
    {
        // Try current directory first
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, EnvFileName)))
        {
            return Environment.CurrentDirectory;
        }
        
        // Try the executable's directory
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        if (File.Exists(Path.Combine(exeDir, EnvFileName)))
        {
            return exeDir;
        }
        
        // Walk up from executable directory looking for .env
        var dir = new DirectoryInfo(exeDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, EnvFileName)))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        
        // Default to current directory
        return Environment.CurrentDirectory;
    }
    
    /// <summary>
    /// Loads configuration from multiple sources
    /// </summary>
    public AppConfig LoadConfiguration()
    {
        var config = new AppConfig();
        
        // 1. Load default values
        LoadDefaults(config);
        
        // 2. Load from appsettings.json
        LoadFromJsonFile(config, Path.Combine(_basePath, ConfigFileName));
        
        // 3. Load from .env file
        LoadFromEnvFile(config, Path.Combine(_basePath, EnvFileName));
        
        // 4. Load from environment variables
        LoadFromEnvironment(config);
        
        // Validate configuration
        ValidateConfiguration(config);
        
        return config;
    }
    
    /// <summary>
    /// Saves configuration to JSON file
    /// </summary>
    public async Task SaveConfigurationAsync(AppConfig config)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(ConfigFileName, json);
            
            _logger.LogInformation($"Configuration saved to {ConfigFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            throw;
        }
    }
    
    private void LoadDefaults(AppConfig config)
    {
        // Default values are already set in the model classes
        _logger.LogDebug("Loaded default configuration values");
    }
    
    private void LoadFromJsonFile(AppConfig config, string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogDebug($"Configuration file {fileName} not found, skipping");
            return;
        }
        
        try
        {
            var json = File.ReadAllText(fileName);
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var fileConfig = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(json, options);
            
            if (fileConfig != null)
            {
                // Merge with existing config
                config.GoogleCloudProjectId = fileConfig.GoogleCloudProjectId ?? config.GoogleCloudProjectId;
                config.GoogleCloudCredentialsPath = fileConfig.GoogleCloudCredentialsPath ?? config.GoogleCloudCredentialsPath;
                config.DefaultOutputDirectory = fileConfig.DefaultOutputDirectory ?? config.DefaultOutputDirectory;
                config.DefaultAudioFormat = fileConfig.DefaultAudioFormat != default ? fileConfig.DefaultAudioFormat : config.DefaultAudioFormat;
                config.EnableCostTracking = fileConfig.EnableCostTracking;
                config.UsageDataFile = fileConfig.UsageDataFile ?? config.UsageDataFile;
                config.VerboseLogging = fileConfig.VerboseLogging;
                
                if (fileConfig.DefaultVoiceSettings != null)
                {
                    config.DefaultVoiceSettings = fileConfig.DefaultVoiceSettings;
                }
                
                if (fileConfig.CostSettings != null)
                {
                    config.CostSettings = fileConfig.CostSettings;
                }
                
                _logger.LogInformation($"Configuration loaded from {fileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load configuration from {fileName}");
        }
    }
    
    private void LoadFromEnvFile(AppConfig config, string fileName)
    {
        if (!File.Exists(fileName))
        {
            _logger.LogDebug($".env file {fileName} not found, skipping");
            return;
        }
        
        try
        {
            // Load .env file
            Env.Load(fileName);
            
            // Apply environment variables
            ApplyEnvironmentVariables(config);
            
            _logger.LogInformation($"Environment variables loaded from {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load .env file {fileName}");
        }
    }
    
    private void LoadFromEnvironment(AppConfig config)
    {
        ApplyEnvironmentVariables(config);
        _logger.LogDebug("Applied system environment variables");
    }
    
    private void ApplyEnvironmentVariables(AppConfig config)
    {
        // Google Cloud settings
        var projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT_ID");
        if (!string.IsNullOrEmpty(projectId))
        {
            config.GoogleCloudProjectId = projectId;
        }
        
        var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        if (!string.IsNullOrEmpty(credentialsPath))
        {
            config.GoogleCloudCredentialsPath = credentialsPath;
        }
        
        // Output settings
        var outputDir = Environment.GetEnvironmentVariable("ADA_OUTPUT_DIRECTORY");
        if (!string.IsNullOrEmpty(outputDir))
        {
            config.DefaultOutputDirectory = outputDir;
        }
        
        // Cost settings
        var costPerChar = Environment.GetEnvironmentVariable("ADA_COST_PER_CHARACTER");
        if (decimal.TryParse(costPerChar, out var cpc))
        {
            config.CostSettings.CostPerCharacter = cpc;
        }
        
        var freeTier = Environment.GetEnvironmentVariable("ADA_FREE_TIER_CHARACTERS");
        if (long.TryParse(freeTier, out var ft))
        {
            config.CostSettings.FreeTierCharacters = ft;
        }
        
        // Logging
        var verbose = Environment.GetEnvironmentVariable("ADA_VERBOSE_LOGGING");
        if (bool.TryParse(verbose, out var v))
        {
            config.VerboseLogging = v;
        }
    }
    
    private void ValidateConfiguration(AppConfig config)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(config.GoogleCloudCredentialsPath))
        {
            errors.Add("Google Cloud credentials path is required");
        }
        else if (!File.Exists(config.GoogleCloudCredentialsPath))
        {
            errors.Add($"Google Cloud credentials file not found: {config.GoogleCloudCredentialsPath}");
        }
        
        if (!string.IsNullOrEmpty(config.DefaultOutputDirectory))
        {
            try
            {
                Path.GetFullPath(config.DefaultOutputDirectory);
            }
            catch
            {
                errors.Add($"Invalid output directory path: {config.DefaultOutputDirectory}");
            }
        }
        
        if (errors.Count > 0)
        {
            var message = "Configuration validation failed:\n" + string.Join("\n", errors);
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }
        
        _logger.LogInformation("Configuration validation passed");
    }
    
    /// <summary>
    /// Creates a sample .env file
    /// </summary>
    public async Task CreateSampleEnvFileAsync()
    {
        var sampleEnv = @"# ADA Voice Creator Configuration
# Copy this file to .env and fill in your values

# Google Cloud Settings
GOOGLE_CLOUD_PROJECT_ID=your-project-id
GOOGLE_APPLICATION_CREDENTIALS=path/to/your/credentials.json

# Output Settings
ADA_OUTPUT_DIRECTORY=output

# Cost Settings (optional)
ADA_COST_PER_CHARACTER=0.000004
ADA_FREE_TIER_CHARACTERS=4000000

# Logging (optional)
ADA_VERBOSE_LOGGING=false
";
        
        await File.WriteAllTextAsync(".env.sample", sampleEnv);
        _logger.LogInformation("Sample .env file created: .env.sample");
    }
}
