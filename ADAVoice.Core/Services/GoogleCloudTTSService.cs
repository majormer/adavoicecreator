using Google.Cloud.TextToSpeech.V1;
using Google.Api.Gax;
using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;
using Google.Api;

namespace ADAVoice.Core.Services;

/// <summary>
/// Google Cloud Text-to-Speech service implementation
/// </summary>
public class GoogleCloudTTSService : ITTSService
{
    private readonly TextToSpeechClient _client;
    private readonly AppConfig _config;
    private readonly ILogger<GoogleCloudTTSService> _logger;
    
    public GoogleCloudTTSService(AppConfig config, ILogger<GoogleCloudTTSService> logger)
    {
        _config = config;
        _logger = logger;
        
        try
        {
            // Create the client
            var clientBuilder = new TextToSpeechClientBuilder();
            
            // Set credentials using GoogleCredential for better security
            if (!string.IsNullOrEmpty(config.GoogleCloudCredentialsPath))
            {
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(config.GoogleCloudCredentialsPath);
                clientBuilder.Credential = credential;
            }
            
            _client = clientBuilder.Build();
            _logger.LogInformation("Google Cloud TTS client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Cloud TTS client");
            throw new InvalidOperationException("Failed to initialize TTS service. Check your credentials.", ex);
        }
    }
    
    public async Task<AudioGenerationResult> GenerateAudioAsync(AudioRequest request)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Validate request
            if (!request.IsValid(out string validationError))
            {
                return AudioGenerationResult.Failure(validationError);
            }
            
            // Use provided voice settings or defaults
            var voiceSettings = request.VoiceSettings ?? _config.DefaultVoiceSettings;
            
            // Create synthesis input
            var synthesisInput = new SynthesisInput
            {
                Text = request.Text
            };
            
            // Configure voice selection
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = voiceSettings.LanguageCode,
                Name = voiceSettings.VoiceName
            };
            
            // Configure audio settings
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                SpeakingRate = (float)voiceSettings.SpeakingRate,
                Pitch = (float)voiceSettings.Pitch,
                VolumeGainDb = (float)voiceSettings.VolumeGainDb,
                SampleRateHertz = voiceSettings.SampleRate
            };
            
            // Generate speech
            _logger.LogInformation($"Generating audio for {request.CharacterCount} characters");
            var response = await _client.SynthesizeSpeechAsync(synthesisInput, voiceSelection, audioConfig);
            
            // Generate output path if not provided
            var outputPath = request.OutputPath;
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = GenerateOutputPath(request);
            }
            
            // Ensure output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            
            // Save audio file
            await File.WriteAllBytesAsync(outputPath, response.AudioContent.ToByteArray());
            
            var generationTime = DateTime.UtcNow - startTime;
            var cost = _config.CostSettings.CostPerCharacter * request.CharacterCount;
            
            _logger.LogInformation($"Audio generated successfully in {generationTime.TotalSeconds:F2}s");
            
            return AudioGenerationResult.Success(
                outputPath,
                request.CharacterCount,
                cost,
                generationTime,
                request.Format
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate audio");
            return AudioGenerationResult.Failure($"Error generating audio: {ex.Message}");
        }
    }
    
    public async Task<decimal> EstimateCostAsync(string text)
    {
        await Task.CompletedTask; // Sync operation, but keeping async for interface consistency
        return _config.CostSettings.CostPerCharacter * text.Length;
    }
    
    public async Task<List<VoiceInfo>> GetAvailableVoicesAsync(string? languageCode = null)
    {
        try
        {
            var request = new ListVoicesRequest();
            if (!string.IsNullOrEmpty(languageCode))
            {
                request.LanguageCode = languageCode;
            }
            
            var response = await _client.ListVoicesAsync(request);
            
            return response.Voices.Select(v => new VoiceInfo
            {
                Name = v.Name,
                LanguageCode = v.LanguageCodes.FirstOrDefault() ?? string.Empty,
                DisplayName = v.Name,
                Gender = v.SsmlGender.ToString(),
                IsNeural = v.NaturalSampleRateHertz > 0,
                NaturalSampleRateHz = v.NaturalSampleRateHertz
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve available voices");
            return new List<VoiceInfo>();
        }
    }
    
    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_config.GoogleCloudCredentialsPath) && 
               File.Exists(_config.GoogleCloudCredentialsPath);
    }
    
    private string GenerateOutputPath(AudioRequest request)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var safeText = string.Join("", 
            request.Text.Take(30)
                .Select(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_' ? c : '_')
                .ToArray())
            .Replace(' ', '_')
            .Trim('_');
        
        var extension = request.Format switch
        {
            AudioFormat.Wav => "wav",
            AudioFormat.Ogg => "ogg",
            _ => "mp3"
        };
        
        return Path.Combine(request.OutputDirectory, $"ada_{timestamp}_{safeText}.{extension}");
    }
}
