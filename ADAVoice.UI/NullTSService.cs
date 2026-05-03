using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;

namespace ADAVoice.UI;

/// <summary>
/// Null implementation of ITTSService that returns appropriate error messages
/// when credentials are not configured. Allows the UI to load without valid credentials.
/// </summary>
public class NullTSService : Core.Services.ITTSService
{
    private readonly ILogger<NullTSService> _logger;

    public NullTSService(ILogger<NullTSService> logger)
    {
        _logger = logger;
    }

    public Task<AudioGenerationResult> GenerateAudioAsync(AudioRequest request)
    {
        _logger.LogWarning("Attempted to generate audio without valid credentials");
        return Task.FromResult(AudioGenerationResult.Failure(
            "Google Cloud credentials are not configured. Please configure your credentials in Settings."));
    }

    public Task<decimal> EstimateCostAsync(string text)
    {
        // Still allow cost estimation even without credentials
        return Task.FromResult(0.000004m * text.Length); // Standard Google Cloud pricing
    }

    public Task<List<VoiceInfo>> GetAvailableVoicesAsync(string? languageCode = null)
    {
        _logger.LogWarning("Attempted to list voices without valid credentials");
        return Task.FromResult(new List<VoiceInfo>());
    }

    public bool IsConfigured()
    {
        return false;
    }
}
