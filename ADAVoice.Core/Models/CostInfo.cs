namespace ADAVoice.Core.Models;

/// <summary>
/// Represents cost information for TTS usage
/// </summary>
public class CostInfo
{
    /// <summary>
    /// Total characters used
    /// </summary>
    public long TotalCharacters { get; set; }
    
    /// <summary>
    /// Characters used in current month
    /// </summary>
    public long MonthlyCharacters { get; set; }
    
    /// <summary>
    /// Total cost in USD
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Monthly cost in USD
    /// </summary>
    public decimal MonthlyCost { get; set; }
    
    /// <summary>
    /// Free tier characters per month
    /// </summary>
    public long FreeTierLimit { get; set; } = 4000000;
    
    /// <summary>
    /// Cost per character in USD
    /// </summary>
    public decimal CostPerCharacter { get; set; } = 0.000004m;
    
    /// <summary>
    /// Characters remaining in free tier
    /// </summary>
    public long FreeTierRemaining => Math.Max(0, FreeTierLimit - MonthlyCharacters);
    
    /// <summary>
    /// Whether the user is over the free tier
    /// </summary>
    public bool IsOverFreeTier => MonthlyCharacters > FreeTierLimit;
    
    /// <summary>
    /// Daily usage breakdown
    /// </summary>
    public Dictionary<string, long> DailyUsage { get; set; } = new();
    
    /// <summary>
    /// Current month in YYYY-MM format
    /// </summary>
    public string CurrentMonth { get; set; } = DateTime.Now.ToString("yyyy-MM");
    
    /// <summary>
    /// Estimates cost for a given number of characters
    /// </summary>
    public decimal EstimateCost(int characterCount)
    {
        return characterCount * CostPerCharacter;
    }
    
    /// <summary>
    /// Creates a copy of this CostInfo instance
    /// </summary>
    public CostInfo Clone()
    {
        return new CostInfo
        {
            TotalCharacters = this.TotalCharacters,
            MonthlyCharacters = this.MonthlyCharacters,
            TotalCost = this.TotalCost,
            MonthlyCost = this.MonthlyCost,
            FreeTierLimit = this.FreeTierLimit,
            CostPerCharacter = this.CostPerCharacter,
            DailyUsage = new Dictionary<string, long>(this.DailyUsage),
            CurrentMonth = this.CurrentMonth
        };
    }
    
    /// <summary>
    /// Gets a formatted cost summary
    /// </summary>
    public string GetSummary()
    {
        return $"Total Characters: {TotalCharacters:N0}\n" +
               $"Monthly Characters: {MonthlyCharacters:N0}\n" +
               $"Total Cost: ${TotalCost:F4}\n" +
               $"Monthly Cost: ${MonthlyCost:F4}\n" +
               $"Free Tier Remaining: {FreeTierRemaining:N0} characters";
    }
}
