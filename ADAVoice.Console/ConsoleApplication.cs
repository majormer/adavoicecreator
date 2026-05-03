using ADAVoice.Core.Services;
using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ADAVoice.Console;

/// <summary>
/// Main console application logic
/// </summary>
public class ConsoleApplication
{
    private readonly ADAVoiceService _adaVoiceService;
    private readonly ILogger<ConsoleApplication> _logger;
    
    public ConsoleApplication(ADAVoiceService adaVoiceService, ILogger<ConsoleApplication> logger)
    {
        _adaVoiceService = adaVoiceService;
        _logger = logger;
    }
    
    public async Task<int> RunAsync(CommandLineOptions options)
    {
        try
        {
            // Show help
            if (options.ShowHelp)
            {
                ShowHelp();
                return 0;
            }
            
            // List voices
            if (options.ListVoices)
            {
                await ListVoicesAsync();
                return 0;
            }
            
            // Show cost info
            if (options.ShowCostInfo)
            {
                await ShowCostInfoAsync();
                return 0;
            }
            
            // Validate configuration
            if (!await _adaVoiceService.ValidateConfigurationAsync())
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Error: ADA Voice Creator is not properly configured.");
                System.Console.WriteLine("Please check your Google Cloud credentials and try again.");
                System.Console.ResetColor();
                return 1;
            }
            
            // Process input
            if (!string.IsNullOrEmpty(options.InputFile))
            {
                return await ProcessBatchFileAsync(options);
            }
            else if (!string.IsNullOrEmpty(options.Text))
            {
                return await ProcessSingleTextAsync(options);
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("No input specified. Use --help for usage information.");
                return 1;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = System.ConsoleColor.Red;
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.ResetColor();
            return 1;
        }
    }
    
    private async Task<int> ProcessSingleTextAsync(CommandLineOptions options)
    {
        var request = new AudioRequest
        {
            Text = options.Text!,
            Format = options.Format,
            OutputPath = options.OutputFile,
            OutputDirectory = options.OutputDirectory ?? "output",
            EstimateCostOnly = options.EstimateCostOnly
        };
        
        var result = await _adaVoiceService.GenerateAudioAsync(request);
        
        if (result.IsSuccess)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("✓ " + result.GetSummary());
            System.Console.ResetColor();
            
            if (!string.IsNullOrEmpty(result.OutputPath))
            {
                System.Console.WriteLine($"Output: {Path.GetFullPath(result.OutputPath)}");
            }
            
            return 0;
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ {result.ErrorMessage}");
            System.Console.ResetColor();
            return 1;
        }
    }
    
    private async Task<int> ProcessBatchFileAsync(CommandLineOptions options)
    {
        if (!File.Exists(options.InputFile))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: Input file not found: {options.InputFile}");
            System.Console.ResetColor();
            return 1;
        }
        
        var lines = await File.ReadAllLinesAsync(options.InputFile);
        var texts = lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")).ToList();
        
        if (texts.Count == 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("No valid text found in input file.");
            System.Console.ResetColor();
            return 0;
        }
        
        System.Console.WriteLine($"Processing {texts.Count} texts from {options.InputFile}...");
        
        var results = await _adaVoiceService.GenerateBatchAsync(
            texts, 
            options.Format, 
            options.OutputDirectory);
        
        var successCount = results.Count(r => r.IsSuccess);
        var totalCost = results.Where(r => r.IsSuccess).Sum(r => r.Cost);
        
        System.Console.WriteLine();
        System.Console.ForegroundColor = successCount == results.Count ? ConsoleColor.Green : ConsoleColor.Yellow;
        System.Console.WriteLine($"Batch processing complete: {successCount}/{results.Count} successful");
        System.Console.WriteLine($"Total cost: ${totalCost:F6}");
        System.Console.ResetColor();
        
        if (successCount < results.Count)
        {
            System.Console.WriteLine("\nFailed items:");
            foreach (var result in results.Where(r => !r.IsSuccess))
            {
                System.Console.WriteLine($"  ✗ {result.ErrorMessage}");
            }
        }
        
        return successCount == results.Count ? 0 : 1;
    }
    
    private async Task ListVoicesAsync()
    {
        System.Console.WriteLine("Available voices:");
        System.Console.WriteLine("================");
        
        var voices = await _adaVoiceService.GetAvailableVoicesAsync("en-US");
        
        foreach (var voice in voices.Where(v => v.IsNeural))
        {
            System.Console.WriteLine($"• {voice.Name} ({voice.DisplayName}) - {voice.Gender}");
        }
    }
    
    private async Task ShowCostInfoAsync()
    {
        var costInfo = await _adaVoiceService.GetCostInfoAsync();
        
        System.Console.WriteLine("Cost Information:");
        System.Console.WriteLine("=================");
        System.Console.WriteLine(costInfo.GetSummary());
        
        if (costInfo.DailyUsage.Any())
        {
            System.Console.WriteLine("\nDaily Usage:");
            foreach (var (date, chars) in costInfo.DailyUsage.OrderByDescending(x => x.Key))
            {
                System.Console.WriteLine($"  {date}: {chars:N0} characters");
            }
        }
    }
    
    private void ShowHelp()
    {
        System.Console.WriteLine("ADA Voice Creator - Console Application");
        System.Console.WriteLine("======================================");
        System.Console.WriteLine();
        System.Console.WriteLine("Usage:");
        System.Console.WriteLine("  ADAVoice.Console [options] \"Text to convert\"");
        System.Console.WriteLine("  ADAVoice.Console --input <file> [options]");
        System.Console.WriteLine();
        System.Console.WriteLine("Options:");
        System.Console.WriteLine("  -t, --text <text>       Text to convert to speech");
        System.Console.WriteLine("  -i, --input <file>      Input file with phrases (one per line)");
        System.Console.WriteLine("  -o, --output <file>     Output file path");
        System.Console.WriteLine("  -f, --format <format>   Audio format (mp3, wav, ogg) [default: mp3]");
        System.Console.WriteLine("  -d, --output-dir <dir>  Output directory [default: output]");
        System.Console.WriteLine("  -c, --cost              Estimate cost only (don't generate)");
        System.Console.WriteLine("  -v, --voices            List available voices");
        System.Console.WriteLine("  --cost-info              Show cost tracking information");
        System.Console.WriteLine("  --verbose                Enable verbose logging");
        System.Console.WriteLine("  -h, --help               Show this help message");
        System.Console.WriteLine();
        System.Console.WriteLine("Examples:");
        System.Console.WriteLine("  ADAVoice.Console \"FICSIT recommends you proceed with caution.\"");
        System.Console.WriteLine("  ADAVoice.Console --input phrases.txt --format wav");
        System.Console.WriteLine("  ADAVoice.Console --text \"Hello\" --cost");
        System.Console.WriteLine();
        System.Console.WriteLine("Environment Variables:");
        System.Console.WriteLine("  GOOGLE_APPLICATION_CREDENTIALS  Path to Google Cloud credentials JSON");
        System.Console.WriteLine("  GOOGLE_CLOUD_PROJECT_ID         Google Cloud project ID");
        System.Console.WriteLine("  ADA_OUTPUT_DIRECTORY              Default output directory");
    }
}
