using ADAVoice.Core.Services;
using ADAVoice.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ADAVoice.Core;

namespace ADAVoice.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        using var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            // Parse command line arguments
            var options = ParseCommandLine(args);
            
            // Run the appropriate command
            var app = serviceProvider.GetRequiredService<ConsoleApplication>();
            return await app.RunAsync(options);
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.ResetColor();
            return 1;
        }
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton<AppConfig>(provider =>
        {
            var configService = new ConfigurationService(
                provider.GetRequiredService<ILogger<ConfigurationService>>());
            return configService.LoadConfiguration();
        });
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Services
        services.AddSingleton<GoogleCloudTTSService>();
        services.AddSingleton<ITTSService>(provider => provider.GetRequiredService<GoogleCloudTTSService>());
        services.AddSingleton<CostTrackingService>();
        services.AddSingleton<ICostTrackingService>(provider => provider.GetRequiredService<CostTrackingService>());
        services.AddSingleton<AudioConversionService>();
        services.AddSingleton<ADAVoiceService>();
        
        // Console app
        services.AddSingleton<ConsoleApplication>();
    }
    
    private static CommandLineOptions ParseCommandLine(string[] args)
    {
        var options = new CommandLineOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                    
                case "--text":
                case "-t":
                    if (i + 1 < args.Length)
                        options.Text = args[++i];
                    break;
                    
                case "--input":
                case "-i":
                    if (i + 1 < args.Length)
                        options.InputFile = args[++i];
                    break;
                    
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;
                    
                case "--format":
                case "-f":
                    if (i + 1 < args.Length && Enum.TryParse<AudioFormat>(args[++i], true, out var format))
                        options.Format = format;
                    break;
                    
                case "--output-dir":
                case "-d":
                    if (i + 1 < args.Length)
                        options.OutputDirectory = args[++i];
                    break;
                    
                case "--cost":
                case "-c":
                    options.EstimateCostOnly = true;
                    break;
                    
                case "--voices":
                case "-v":
                    options.ListVoices = true;
                    break;
                    
                case "--cost-info":
                    options.ShowCostInfo = true;
                    break;
                    
                case "--verbose":
                    options.Verbose = true;
                    break;
                    
                default:
                    if (!args[i].StartsWith("-") && string.IsNullOrEmpty(options.Text))
                    {
                        options.Text = args[i];
                    }
                    break;
            }
        }
        
        return options;
    }
}

public class CommandLineOptions
{
    public string? Text { get; set; }
    public string? InputFile { get; set; }
    public string? OutputFile { get; set; }
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
    public string? OutputDirectory { get; set; }
    public bool EstimateCostOnly { get; set; }
    public bool ShowHelp { get; set; }
    public bool ListVoices { get; set; }
    public bool ShowCostInfo { get; set; }
    public bool Verbose { get; set; }
}
