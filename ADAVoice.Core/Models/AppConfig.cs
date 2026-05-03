namespace ADAVoice.Core.Models;

/// <summary>
/// Application configuration settings
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Google Cloud project ID
    /// </summary>
    public string GoogleCloudProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to Google Cloud credentials JSON file
    /// </summary>
    public string GoogleCloudCredentialsPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Default output directory
    /// </summary>
    public string DefaultOutputDirectory { get; set; } = "output";
    
    /// <summary>
    /// Default audio format
    /// </summary>
    public AudioFormat DefaultAudioFormat { get; set; } = AudioFormat.Mp3;
    
    /// <summary>
    /// Default voice settings
    /// </summary>
    public VoiceSettings DefaultVoiceSettings { get; set; } = new VoiceSettings();
    
    /// <summary>
    /// Cost tracking settings
    /// </summary>
    public CostSettings CostSettings { get; set; } = new CostSettings();
    
    /// <summary>
    /// Whether to enable cost tracking
    /// </summary>
    public bool EnableCostTracking { get; set; } = true;
    
    /// <summary>
    /// Usage data file path
    /// </summary>
    public string UsageDataFile { get; set; } = "usage_data.json";
    
    /// <summary>
    /// Whether to show verbose logging
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
}

/// <summary>
/// Cost tracking settings
/// </summary>
public class CostSettings
{
    /// <summary>
    /// Cost per character in USD
    /// </summary>
    public decimal CostPerCharacter { get; set; } = 0.000004m;
    
    /// <summary>
    /// Free tier characters per month
    /// </summary>
    public long FreeTierCharacters { get; set; } = 4000000;
    
    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Whether to warn when approaching free tier limit
    /// </summary>
    public bool WarnOnFreeTierLimit { get; set; } = true;
    
    /// <summary>
    /// Percentage threshold for free tier warning (0.0 to 1.0)
    /// </summary>
    public double FreeTierWarningThreshold { get; set; } = 0.9;
}
