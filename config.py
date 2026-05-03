"""
ADA Voice Creator Configuration

Contains all configurable settings for the ADA voice generation.
"""

# ADA Voice Settings (Google Cloud TTS)
ADA_VOICE_CONFIG = {
    "language_code": "en-US",
    "voice_name": "en-US-Wavenet-C",
    "speaking_rate": 0.95,  # Slightly slower for professional tone
    "pitch": 0,  # Neutral pitch
    "volume_gain_db": 0  # No volume adjustment
}

# Output Configuration
OUTPUT_CONFIG = {
    "output_directory": "output",
    "default_format": "mp3",
    "sample_rate": 24000,  # 24kHz for good quality
    "channels": 1  # Mono for voice
}

# Cost Configuration (Google Cloud WaveNet Pricing)
COST_CONFIG = {
    "cost_per_character": 0.000004,  # $4 per 1M characters
    "free_tier_characters": 4000000,  # 4M characters free per month
    "currency": "USD"
}

# Audio Quality Settings
AUDIO_QUALITY = {
    "mp3": {
        "bitrate": "128k",
        "codec": "mp3"
    },
    "wav": {
        "sample_width": 2,  # 16-bit
        "codec": "pcm"
    },
    "ogg": {
        "bitrate": "128k",
        "codec": "libvorbis"
    }
}

# File Naming Patterns
NAMING_PATTERNS = {
    "single": "ada_{timestamp}_{text_preview}.{format}",
    "batch": "batch_{index:03d}.{format}",
    "custom": "{custom_name}.{format}"
}

# Logging Configuration
LOGGING_CONFIG = {
    "level": "INFO",
    "format": "%(asctime)s - %(levelname)s - %(message)s",
    "file": "adavoice.log"
}

# Google Cloud Settings
GOOGLE_CLOUD_CONFIG = {
    "project_id": None,  # Set if different from service account
    "location": "global",  # API endpoint location
    "timeout": 30  # Request timeout in seconds
}
