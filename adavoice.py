#!/usr/bin/env python3
"""
ADA Voice Creator

Generates ADA voice audio files using Google Cloud Text-to-Speech API.
"""

import argparse
import os
import sys
from datetime import datetime
from pathlib import Path
from typing import List, Optional

from google.cloud import texttospeech
from pydub import AudioSegment

from config import ADA_VOICE_CONFIG, OUTPUT_CONFIG, COST_CONFIG
from cost_tracker import CostTracker


class ADAVoiceCreator:
    """Main class for creating ADA voice audio files."""
    
    def __init__(self):
        """Initialize the ADA Voice Creator."""
        self.client = texttospeech.TextToSpeechClient()
        self.cost_tracker = CostTracker()
        self.output_dir = Path(OUTPUT_CONFIG["output_directory"])
        self.output_dir.mkdir(exist_ok=True)
    
    def synthesize_speech(self, text: str, output_filename: Optional[str] = None) -> str:
        """
        Synthesize speech for the given text.
        
        Args:
            text: The text to synthesize
            output_filename: Optional custom output filename
            
        Returns:
            Path to the generated audio file
        """
        # Track character usage
        char_count = len(text)
        self.cost_tracker.add_characters(char_count)
        
        # Configure synthesis input
        synthesis_input = texttospeech.SynthesisInput(text=text)
        
        # Configure voice
        voice = texttospeech.VoiceSelectionParams(
            language_code=ADA_VOICE_CONFIG["language_code"],
            name=ADA_VOICE_CONFIG["voice_name"]
        )
        
        # Configure audio settings
        audio_config = texttospeech.AudioConfig(
            audio_encoding=texttospeech.AudioEncoding.MP3,
            speaking_rate=ADA_VOICE_CONFIG["speaking_rate"],
            pitch=ADA_VOICE_CONFIG["pitch"],
            volume_gain_db=ADA_VOICE_CONFIG["volume_gain_db"]
        )
        
        # Perform synthesis
        print(f"Synthesizing: {text[:50]}{'...' if len(text) > 50 else ''}")
        response = self.client.synthesize_speech(
            input=synthesis_input,
            voice=voice,
            audio_config=audio_config
        )
        
        # Generate output filename
        if not output_filename:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            safe_text = "".join(c for c in text[:30] if c.isalnum() or c in (' ', '-', '_')).rstrip()
            safe_text = safe_text.replace(' ', '_')
            output_filename = f"ada_{timestamp}_{safe_text}.mp3"
        
        # Ensure .mp3 extension
        if not output_filename.endswith('.mp3'):
            output_filename += '.mp3'
        
        # Save audio file
        output_path = self.output_dir / output_filename
        with open(output_path, "wb") as out:
            out.write(response.audio_content)
        
        print(f"Saved: {output_path}")
        print(f"Characters used: {char_count}")
        print(f"Estimated cost: ${char_count * COST_CONFIG['cost_per_character']:.6f}")
        
        return str(output_path)
    
    def batch_process(self, input_file: str) -> List[str]:
        """
        Process multiple phrases from a file.
        
        Args:
            input_file: Path to file containing phrases (one per line)
            
        Returns:
            List of generated audio file paths
        """
        generated_files = []
        
        try:
            with open(input_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            
            for i, line in enumerate(lines, 1):
                text = line.strip()
                if text and not text.startswith('#'):  # Skip empty lines and comments
                    try:
                        output_filename = f"batch_{i:03d}.mp3"
                        output_path = self.synthesize_speech(text, output_filename)
                        generated_files.append(output_path)
                    except Exception as e:
                        print(f"Error processing line {i}: {e}")
        
        except FileNotFoundError:
            print(f"Error: Input file '{input_file}' not found.")
            sys.exit(1)
        except Exception as e:
            print(f"Error reading input file: {e}")
            sys.exit(1)
        
        return generated_files
    
    def convert_format(self, input_file: str, output_format: str) -> str:
        """
        Convert audio file to different format.
        
        Args:
            input_file: Path to input audio file
            output_format: Target format (wav, ogg, etc.)
            
        Returns:
            Path to converted file
        """
        audio = AudioSegment.from_mp3(input_file)
        output_path = input_file.replace('.mp3', f'.{output_format}')
        
        if output_format.lower() == 'wav':
            audio.export(output_path, format="wav")
        elif output_format.lower() == 'ogg':
            audio.export(output_path, format="ogg", codec="libvorbis")
        else:
            print(f"Unsupported format: {output_format}")
            return input_file
        
        print(f"Converted to: {output_path}")
        return output_path
    
    def print_cost_summary(self):
        """Print cost usage summary."""
        summary = self.cost_tracker.get_summary()
        print("\n" + "="*50)
        print("COST SUMMARY")
        print("="*50)
        print(f"Total characters: {summary['total_characters']:,}")
        print(f"Total cost: ${summary['total_cost']:.4f}")
        print(f"Free tier remaining: {summary['free_tier_remaining']:,} characters")
        if summary['free_tier_remaining'] < 0:
            print(f"⚠️  OVER FREE TIER by {abs(summary['free_tier_remaining']):,} characters")
        print("="*50)


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="Generate ADA voice audio files using Google Cloud TTS"
    )
    
    # Text input
    parser.add_argument(
        "text",
        nargs="?",
        help="Text to convert to ADA voice"
    )
    
    # Batch processing
    parser.add_argument(
        "--batch",
        "-b",
        help="Process multiple phrases from a file (one per line)"
    )
    
    # Output options
    parser.add_argument(
        "--output",
        "-o",
        help="Custom output filename"
    )
    
    parser.add_argument(
        "--format",
        "-f",
        choices=["mp3", "wav", "ogg"],
        default="mp3",
        help="Output audio format (default: mp3)"
    )
    
    # Other options
    parser.add_argument(
        "--convert",
        help="Convert existing audio file to different format"
    )
    
    parser.add_argument(
        "--cost",
        action="store_true",
        help="Show cost information only"
    )
    
    args = parser.parse_args()
    
    # Create ADA Voice Creator
    creator = ADAVoiceCreator()
    
    # Show cost information
    if args.cost:
        creator.print_cost_summary()
        return
    
    # Convert existing file
    if args.convert:
        creator.convert_format(args.convert, args.format)
        return
    
    # Batch processing
    if args.batch:
        files = creator.batch_process(args.batch)
        print(f"\nGenerated {len(files)} audio files")
        creator.print_cost_summary()
        return
    
    # Single phrase
    if not args.text:
        parser.error("Text input is required. Use --batch for file input.")
    
    # Generate audio
    output_path = creator.synthesize_speech(args.text, args.output)
    
    # Convert if needed
    if args.format != "mp3":
        output_path = creator.convert_format(output_path, args.format)
    
    # Show summary
    creator.print_cost_summary()


if __name__ == "__main__":
    main()
