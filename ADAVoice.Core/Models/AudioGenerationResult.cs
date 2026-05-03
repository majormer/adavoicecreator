namespace ADAVoice.Core.Models;

/// <summary>
/// Represents the result of an audio generation request
/// </summary>
public class AudioGenerationResult
{
    /// <summary>
    /// Whether the generation was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Path to the generated audio file
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of characters processed
    /// </summary>
    public int CharacterCount { get; set; }
    
    /// <summary>
    /// Estimated cost for this generation
    /// </summary>
    public decimal Cost { get; set; }
    
    /// <summary>
    /// Time taken to generate the audio
    /// </summary>
    public TimeSpan GenerationTime { get; set; }
    
    /// <summary>
    /// Audio format of the generated file
    /// </summary>
    public AudioFormat Format { get; set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static AudioGenerationResult Success(
        string outputPath,
        int characterCount,
        decimal cost,
        TimeSpan generationTime,
        AudioFormat format)
    {
        var fileInfo = new FileInfo(outputPath);
        
        return new AudioGenerationResult
        {
            IsSuccess = true,
            OutputPath = outputPath,
            CharacterCount = characterCount,
            Cost = cost,
            GenerationTime = generationTime,
            Format = format,
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static AudioGenerationResult Failure(string errorMessage)
    {
        return new AudioGenerationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
    
    /// <summary>
    /// Gets a human-readable summary
    /// </summary>
    public string GetSummary()
    {
        if (!IsSuccess)
        {
            return $"Failed: {ErrorMessage}";
        }
        
        return $"Success! Generated {Format.ToString().ToUpper()} file: {Path.GetFileName(OutputPath)}\n" +
               $"Characters: {CharacterCount:N0}\n" +
               $"Cost: ${Cost:F6}\n" +
               $"Time: {GenerationTime.TotalSeconds:F2}s\n" +
               $"Size: {FormatFileSize(FileSizeBytes)}";
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
