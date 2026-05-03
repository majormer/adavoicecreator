using System.ComponentModel.DataAnnotations;

namespace ADAVoice.Core.Models;

/// <summary>
/// Represents voice configuration settings for TTS generation
/// </summary>
public class VoiceSettings
{
    /// <summary>
    /// Language code (e.g., "en-US")
    /// </summary>
    [Required]
    public string LanguageCode { get; set; } = "en-US";
    
    /// <summary>
    /// Voice name (e.g., "en-US-Wavenet-C")
    /// </summary>
    [Required]
    public string VoiceName { get; set; } = "en-US-Wavenet-C";
    
    /// <summary>
    /// Speaking rate (0.25 to 4.0, 1.0 is normal)
    /// </summary>
    [Range(0.25, 4.0)]
    public double SpeakingRate { get; set; } = 0.95;
    
    /// <summary>
    /// Pitch (-20.0 to 20.0, 0.0 is normal)
    /// </summary>
    [Range(-20.0, 20.0)]
    public double Pitch { get; set; } = 0.0;
    
    /// <summary>
    /// Volume gain in dB (-96.0 to 16.0, 0.0 is no change)
    /// </summary>
    [Range(-96.0, 16.0)]
    public double VolumeGainDb { get; set; } = 0.0;
    
    /// <summary>
    /// Sample rate for output audio (in Hz)
    /// </summary>
    public int SampleRate { get; set; } = 24000;
    
    /// <summary>
    /// Creates a copy of this VoiceSettings instance
    /// </summary>
    public VoiceSettings Clone()
    {
        return new VoiceSettings
        {
            LanguageCode = this.LanguageCode,
            VoiceName = this.VoiceName,
            SpeakingRate = this.SpeakingRate,
            Pitch = this.Pitch,
            VolumeGainDb = this.VolumeGainDb,
            SampleRate = this.SampleRate
        };
    }
    
    /// <summary>
    /// Validates the voice settings
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(this, validationContext, validationResults);
        
        if (!isValid)
        {
            errorMessage = string.Join(Environment.NewLine, 
                validationResults.Select(r => r.ErrorMessage));
            return false;
        }
        
        errorMessage = string.Empty;
        return true;
    }
}
