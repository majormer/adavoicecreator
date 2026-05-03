using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;

namespace ADAVoice.Core.Services;

/// <summary>
/// Main service for ADA Voice generation
/// </summary>
public class ADAVoiceService
{
    private readonly ITTSService _ttsService;
    private readonly ICostTrackingService _costTrackingService;
    private readonly AudioConversionService _audioConversionService;
    private readonly AppConfig _config;
    private readonly ILogger<ADAVoiceService> _logger;
    
    public ADAVoiceService(
        ITTSService ttsService,
        ICostTrackingService costTrackingService,
        AudioConversionService audioConversionService,
        AppConfig config,
        ILogger<ADAVoiceService> logger)
    {
        _ttsService = ttsService;
        _costTrackingService = costTrackingService;
        _audioConversionService = audioConversionService;
        _config = config;
        _logger = logger;
    }
    
    /// <summary>
    /// Generates audio from text with full tracking
    /// </summary>
    public async Task<AudioGenerationResult> GenerateAudioAsync(AudioRequest request)
    {
        try
        {
            _logger.LogInformation($"Starting audio generation for {request.CharacterCount} characters");
            
            // Check if cost estimation only
            if (request.EstimateCostOnly)
            {
                var cost = await _ttsService.EstimateCostAsync(request.Text);
                _logger.LogInformation($"Estimated cost: ${cost:F6}");
                return new AudioGenerationResult
                {
                    IsSuccess = true,
                    CharacterCount = request.CharacterCount,
                    Cost = cost,
                    GenerationTime = TimeSpan.Zero,
                    Format = request.Format
                };
            }
            
            // Validate request
            if (!request.IsValid(out string validationError))
            {
                return AudioGenerationResult.Failure(validationError);
            }
            
            // Check service configuration
            if (!_ttsService.IsConfigured())
            {
                return AudioGenerationResult.Failure("TTS service is not properly configured");
            }
            
            // Generate audio
            var result = await _ttsService.GenerateAudioAsync(request);
            
            // Track usage if successful
            if (result.IsSuccess)
            {
                await _costTrackingService.AddUsageAsync(result.CharacterCount);
                
                // Convert to different format if needed
                if (request.Format != AudioFormat.Mp3 && !string.IsNullOrEmpty(result.OutputPath))
                {
                    try
                    {
                        var convertedPath = await _audioConversionService.ConvertAsync(
                            result.OutputPath, 
                            request.Format);
                        
                        // Update result with converted file info
                        result.OutputPath = convertedPath;
                        result.Format = request.Format;
                        
                        // Update file size
                        var fileInfo = new FileInfo(convertedPath);
                        result.FileSizeBytes = fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert audio format, using original MP3");
                    }
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during audio generation");
            return AudioGenerationResult.Failure($"Unexpected error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Generates audio for multiple texts (batch processing)
    /// </summary>
    public async Task<List<AudioGenerationResult>> GenerateBatchAsync(
        IEnumerable<string> texts,
        AudioFormat format = AudioFormat.Mp3,
        string? outputDirectory = null)
    {
        var results = new List<AudioGenerationResult>();
        var outputDir = outputDirectory ?? _config.DefaultOutputDirectory;
        
        _logger.LogInformation($"Starting batch generation for {texts.Count()} texts");
        
        var index = 1;
        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text))
                continue;
                
            var request = new AudioRequest
            {
                Text = text,
                Format = format,
                OutputDirectory = outputDir,
                OutputPath = Path.Combine(outputDir, $"batch_{index:D3}.{format.ToString().ToLower()}")
            };
            
            var result = await GenerateAudioAsync(request);
            results.Add(result);
            
            index++;
            
            // Add small delay to avoid rate limiting
            await Task.Delay(100);
        }
        
        var successCount = results.Count(r => r.IsSuccess);
        _logger.LogInformation($"Batch generation complete: {successCount}/{results.Count} successful");
        
        return results;
    }
    
    /// <summary>
    /// Gets current cost information
    /// </summary>
    public async Task<CostInfo> GetCostInfoAsync()
    {
        return await _costTrackingService.GetCostInfoAsync();
    }
    
    /// <summary>
    /// Estimates cost for text without generating audio
    /// </summary>
    public async Task<decimal> EstimateCostAsync(string text)
    {
        return await _ttsService.EstimateCostAsync(text);
    }
    
    /// <summary>
    /// Gets available voices
    /// </summary>
    public async Task<List<VoiceInfo>> GetAvailableVoicesAsync(string? languageCode = null)
    {
        return await _ttsService.GetAvailableVoicesAsync(languageCode);
    }
    
    /// <summary>
    /// Validates that all services are properly configured
    /// </summary>
    public async Task<bool> ValidateConfigurationAsync()
    {
        try
        {
            // Check TTS service
            if (!_ttsService.IsConfigured())
            {
                _logger.LogError("TTS service is not configured");
                return false;
            }
            
            // Test TTS service with minimal request
            var testRequest = new AudioRequest
            {
                Text = "Test",
                EstimateCostOnly = true
            };
            
            var testResult = await _ttsService.GenerateAudioAsync(testRequest);
            if (!testResult.IsSuccess)
            {
                _logger.LogError($"TTS service test failed: {testResult.ErrorMessage}");
                return false;
            }
            
            // Check output directory
            try
            {
                Directory.CreateDirectory(_config.DefaultOutputDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot create output directory: {_config.DefaultOutputDirectory}");
                return false;
            }
            
            _logger.LogInformation("All services are properly configured");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            return false;
        }
    }
}
