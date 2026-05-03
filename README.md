# ADA Voice Creator

A cross-platform tool for creating ADA voice audio files using Google Cloud Text-to-Speech API. Available in both Python and C# versions.

## Overview

This tool allows you to:
- Enter text phrases and generate ADA voice audio files
- Batch process multiple lines
- Apply ADA's specific voice settings (US-Wavenet-C with custom parameters)
- Save audio in various formats (MP3, WAV, OGG)
- Track usage and costs

## Available Versions

### Python Version
- Simple script-based implementation
- Quick setup and usage
- Cross-platform compatibility

### C# Version (.NET 8)
- Full console application with rich features
- Windows Forms UI with batch processing
- Advanced configuration and dependency injection
- Better suited for integration and automation

---

## Python Version (Original)

### Prerequisites
- Python 3.8 or higher
- Google Cloud account with Text-to-Speech API enabled
- Google Cloud credentials

### Quick Start
```bash
# Install dependencies
pip install -r requirements.txt

# Set credentials
set GOOGLE_APPLICATION_CREDENTIALS=path/to/your/key.json

# Generate audio
python adavoice.py "FICSIT recommends you proceed with caution."
```

---

## C# Version (.NET 8)

### Prerequisites
- .NET 8.0 or higher
- Google Cloud account with Text-to-Speech API enabled
- Google Cloud service account credentials (JSON file)

### Setup

1. Build the solution:
```bash
dotnet build
```

2. Configure credentials:
   - Copy `.env.sample` to `.env`
   - Fill in your Google Cloud credentials:
   ```
   GOOGLE_APPLICATION_CREDENTIALS=path/to/your/credentials.json
   GOOGLE_CLOUD_PROJECT_ID=your-project-id
   ```

### Console Application Usage

#### Single Phrase:
```bash
dotnet run --project ADAVoice.Console "FICSIT recommends you proceed with caution."
```

#### Batch Processing:
```bash
dotnet run --project ADAVoice.Console --input sample_phrases.txt
```

#### Custom Output:
```bash
dotnet run --project ADAVoice.Console --text "Hello" --output custom.wav --format wav
```

#### Cost Estimation:
```bash
dotnet run --project ADAVoice.Console --text "Your text" --cost
```

#### View Available Voices:
```bash
dotnet run --project ADAVoice.Console --voices
```

### Command Line Options

```
Options:
  -t, --text <text>       Text to convert to speech
  -i, --input <file>      Input file with phrases (one per line)
  -o, --output <file>     Output file path
  -f, --format <format>   Audio format (mp3, wav, ogg) [default: mp3]
  -d, --output-dir <dir>  Output directory [default: output]
  -c, --cost              Estimate cost only
  -v, --voices            List available voices
  --cost-info              Show cost tracking information
  --verbose                Enable verbose logging
  -h, --help               Show help message
```

## Configuration

The C# version loads configuration from multiple sources (in order of precedence):
1. Environment variables
2. .env file
3. appsettings.json
4. Default values

### Configuration Options

```json
{
  "GoogleCloudProjectId": "your-project-id",
  "GoogleCloudCredentialsPath": "path/to/credentials.json",
  "DefaultOutputDirectory": "output",
  "DefaultAudioFormat": "Mp3",
  "EnableCostTracking": true,
  "DefaultVoiceSettings": {
    "LanguageCode": "en-US",
    "VoiceName": "en-US-Wavenet-C",
    "SpeakingRate": 0.95,
    "Pitch": 0.0,
    "VolumeGainDb": 0.0,
    "SampleRate": 24000
  },
  "CostSettings": {
    "CostPerCharacter": 0.000004,
    "FreeTierCharacters": 4000000,
    "Currency": "USD"
  }
}
```

## Windows Forms Application

The C# version also includes a full-featured Windows Forms UI:

### Running the UI

```bash
# Using the batch file
run-ui.bat

# Or directly with dotnet
dotnet run --project ADAVoice.UI
```

### UI Features

1. **Main Window**:
   - Text input area with character count
   - Voice settings controls (rate, pitch, volume)
   - Output format selection (MP3, WAV, OGG)
   - Output file/directory selection
   - Real-time cost tracking
   - Progress indicator

2. **Batch Processing**:
   - Process multiple phrases from a text file
   - Configurable delay between requests
   - Progress tracking with results list
   - Stop on error option
   - Total cost calculation

3. **Settings Dialog**:
   - Google Cloud credentials configuration
   - Default voice settings
   - Cost tracking preferences
   - Usage statistics display
   - Links to documentation

4. **Additional Features**:
   - Play generated audio files
   - Auto-generate filenames
   - Open output folder
   - Cost estimation before generation
   - Voice selection from available WaveNet voices

### Configuration

The UI uses the same configuration sources as the console app:
- `.env` file (highest priority)
- `appsettings.json`
- Environment variables
- Default values

### Keyboard Shortcuts

- `Ctrl+Enter`: Generate audio
- `Ctrl+E`: Estimate cost
- `Ctrl+B`: Browse for output file
- `F5`: Refresh usage data (in settings)

## Project Structure

```
ADAVoiceCreator/
├── Python Version/
│   ├── adavoice.py          # Main script
│   ├── config.py            # Configuration
│   ├── cost_tracker.py      # Cost tracking
│   └── requirements.txt     # Dependencies
├── C# Version/
│   ├── ADAVoice.Core/       # Core library
│   │   ├── Models/         # Data models
│   │   ├── Services/       # TTS and other services
│   │   └── Interfaces/     # Service interfaces
│   ├── ADAVoice.Console/   # Console application
│   └── ADAVoice.UI/        # Windows Forms UI
├── .env.sample             # Environment template
├── appsettings.json        # C# configuration
└── sample_phrases.txt      # Sample batch file
```

## Cost Information

Google Cloud WaveNet pricing:
- **Free tier**: 4 million characters per month
- **Paid tier**: $4 per 1 million characters
- ADA voice uses approximately 1 character per byte of text

## ADA Voice Settings

- **Voice**: en-US-Wavenet-C
- **Speaking Rate**: 0.95
- **Pitch**: 0
- **Volume Gain**: 0dB

## License

MIT License - see LICENSE file for details

## VirusTotal False Positives

**Why releases may trigger antivirus warnings:**

The self-contained .NET executables in this project's releases may occasionally trigger false positives on virus scanners like VirusTotal. This is a known and common issue with self-contained .NET applications and does **not** indicate actual malware.

**Technical reasons:**
- **Large file size** (60-100MB+) - Bundling the entire .NET runtime triggers heuristic analysis about suspicious file sizes
- **Embedded resources** - The .NET CLR, libraries, and dependencies are embedded as resources, which can appear similar to packing or obfuscation techniques used by malware
- **Unsigned binaries** - The executables are not code-signed (requires paid certificate), making new executables suspicious to some antivirus engines
- **Complex PE structure** - Self-contained .NET apps have an unusual structure compared to typical Windows executables
- **Common dependencies** - Libraries like Google Cloud APIs, Newtonsoft JSON, and other widely-used components can trigger generic pattern matching

**If you have concerns:**
- The complete source code is available on GitHub for your review
- You can build the application yourself using the provided build instructions
- The project is open source under the MIT license
- All dependencies are well-maintained, open-source, or official Google Cloud libraries

**To build the application yourself:**
```bash
# Clone the repository
git clone https://github.com/majormer/adavoicecreator.git
cd adavoicecreator

# Build the solution
dotnet build

# Run the console app
dotnet run --project ADAVoice.Console

# Run the UI app
dotnet run --project ADAVoice.UI
```

**Verification:**
- All code is publicly available on GitHub
- Dependencies are listed in `.csproj` files
- No obfuscation or packing is used beyond standard .NET compilation
- The application only makes legitimate Google Cloud TTS API calls and file operations
