"""
Cost Tracker for ADA Voice Creator

Tracks character usage and calculates costs for Google Cloud TTS.
"""

import json
from datetime import datetime, timedelta
from pathlib import Path
from typing import Dict, Any

from config import COST_CONFIG


class CostTracker:
    """Tracks TTS usage and calculates costs."""
    
    def __init__(self, data_file: str = "usage_data.json"):
        """
        Initialize the cost tracker.
        
        Args:
            data_file: JSON file to store usage data
        """
        self.data_file = Path(data_file)
        self.usage_data = self._load_usage_data()
    
    def _load_usage_data(self) -> Dict[str, Any]:
        """Load usage data from file."""
        if self.data_file.exists():
            try:
                with open(self.data_file, 'r') as f:
                    data = json.load(f)
                
                # Reset if it's a new month
                current_month = datetime.now().strftime("%Y-%m")
                if data.get("month") != current_month:
                    data = {
                        "month": current_month,
                        "total_characters": 0,
                        "daily_usage": {}
                    }
                
                return data
            except (json.JSONDecodeError, KeyError):
                pass
        
        # Return default structure
        return {
            "month": datetime.now().strftime("%Y-%m"),
            "total_characters": 0,
            "daily_usage": {}
        }
    
    def _save_usage_data(self):
        """Save usage data to file."""
        with open(self.data_file, 'w') as f:
            json.dump(self.usage_data, f, indent=2)
    
    def add_characters(self, count: int):
        """
        Add character count to usage tracking.
        
        Args:
            count: Number of characters used
        """
        today = datetime.now().strftime("%Y-%m-%d")
        
        # Update daily usage
        if today not in self.usage_data["daily_usage"]:
            self.usage_data["daily_usage"][today] = 0
        self.usage_data["daily_usage"][today] += count
        
        # Update total
        self.usage_data["total_characters"] += count
        
        # Save to file
        self._save_usage_data()
    
    def get_summary(self) -> Dict[str, Any]:
        """
        Get usage and cost summary.
        
        Returns:
            Dictionary with usage statistics
        """
        total_characters = self.usage_data["total_characters"]
        total_cost = total_characters * COST_CONFIG["cost_per_character"]
        free_tier_remaining = COST_CONFIG["free_tier_characters"] - total_characters
        
        return {
            "total_characters": total_characters,
            "total_cost": total_cost,
            "free_tier_remaining": free_tier_remaining,
            "currency": COST_CONFIG["currency"],
            "month": self.usage_data["month"],
            "daily_usage": self.usage_data["daily_usage"]
        }
    
    def get_daily_breakdown(self) -> Dict[str, int]:
        """
        Get daily usage breakdown.
        
        Returns:
            Dictionary mapping dates to character counts
        """
        return self.usage_data.get("daily_usage", {})
    
    def estimate_cost(self, text: str) -> float:
        """
        Estimate cost for a given text.
        
        Args:
            text: Text to estimate cost for
            
        Returns:
            Estimated cost in USD
        """
        return len(text) * COST_CONFIG["cost_per_character"]
    
    def can_afford(self, text: str, budget: float) -> bool:
        """
        Check if text can be generated within budget.
        
        Args:
            text: Text to check
            budget: Available budget
            
        Returns:
            True if affordable
        """
        estimated_cost = self.estimate_cost(text)
        current_total = self.usage_data["total_characters"] * COST_CONFIG["cost_per_character"]
        return (current_total + estimated_cost) <= budget
