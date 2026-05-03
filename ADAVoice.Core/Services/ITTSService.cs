using ADAVoice.Core.Models;

namespace ADAVoice.Core.Services;

/// <summary>
/// Interface for Text-to-Speech services
/// </summary>
public interface ITTSService
{
    /// <summary>
    /// Generates audio from text
    /// </summary>
    /// <param name="request">The audio generation request</param>
    /// <returns>Result of the audio generation</returns>
    Task<AudioGenerationResult> GenerateAudioAsync(AudioRequest request);
    
    /// <summary>
    /// Estimates the cost for generating audio from text
    /// </summary>
    /// <param name="text">Text to estimate cost for</param>
    /// <returns>Estimated cost in USD</returns>
    Task<decimal> EstimateCostAsync(string text);
    
    /// <summary>
    /// Gets available voices
    /// </summary>
    /// <param name="languageCode">Language code filter (optional)</param>
    /// <returns>List of available voices</returns>
    Task<List<VoiceInfo>> GetAvailableVoicesAsync(string? languageCode = null);
    
    /// <summary>
    /// Validates the service configuration
    /// </summary>
    /// <returns>True if properly configured</returns>
    bool IsConfigured();
}

/// <summary>
/// Information about an available voice
/// </summary>
public class VoiceInfo
{
    /// <summary>
    /// Voice name (e.g., "en-US-Wavenet-C")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Language code (e.g., "en-US")
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the voice
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gender of the voice (if available)
    /// </summary>
    public string? Gender { get; set; }
    
    /// <summary>
    /// Whether this is a neural/WaveNet voice
    /// </summary>
    public bool IsNeural { get; set; }
    
    /// <summary>
    /// Natural language sample rate
    /// </summary>
    public int NaturalSampleRateHz { get; set; }
}
