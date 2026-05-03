using System.ComponentModel.DataAnnotations;

namespace ADAVoice.Core.Models;

/// <summary>
/// Represents a request to generate audio from text
/// </summary>
public class AudioRequest
{
    /// <summary>
    /// Text to convert to speech
    /// </summary>
    [Required]
    [StringLength(5000, ErrorMessage = "Text cannot exceed 5000 characters")]
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Output file path (optional - will auto-generate if not provided)
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// Output audio format
    /// </summary>
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
    
    /// <summary>
    /// Output directory (defaults to "output")
    /// </summary>
    public string OutputDirectory { get; set; } = "output";
    
    /// <summary>
    /// Voice settings to use (optional - will use defaults if not provided)
    /// </summary>
    public VoiceSettings? VoiceSettings { get; set; }
    
    /// <summary>
    /// Whether to estimate cost only without generating audio
    /// </summary>
    public bool EstimateCostOnly { get; set; } = false;
    
    /// <summary>
    /// Gets the character count of the text
    /// </summary>
    public int CharacterCount => Text?.Length ?? 0;
    
    /// <summary>
    /// Validates the audio request
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

/// <summary>
/// Supported audio formats
/// </summary>
public enum AudioFormat
{
    Mp3,
    Wav,
    Ogg
}
