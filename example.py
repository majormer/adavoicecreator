#!/usr/bin/env python3
"""
Example usage of ADA Voice Creator
"""

from adavoice import ADAVoiceCreator


def main():
    """Example usage scenarios."""
    
    # Initialize the creator
    creator = ADAVoiceCreator()
    
    # Example 1: Single phrase
    print("Example 1: Single phrase")
    print("-" * 40)
    text = "FICSIT recommends you proceed with caution."
    output = creator.synthesize_speech(text)
    print(f"Generated: {output}\n")
    
    # Example 2: Batch processing
    print("Example 2: Batch processing")
    print("-" * 40)
    files = creator.batch_process("sample_phrases.txt")
    print(f"Generated {len(files)} files\n")
    
    # Example 3: Custom output with different format
    print("Example 3: Custom output (WAV format)")
    print("-" * 40)
    text = "Efficiency is paramount."
    output = creator.synthesize_speech(text, "efficiency_warning.mp3")
    wav_output = creator.convert_format(output, "wav")
    print(f"Generated: {wav_output}\n")
    
    # Show cost summary
    creator.print_cost_summary()


if __name__ == "__main__":
    main()
