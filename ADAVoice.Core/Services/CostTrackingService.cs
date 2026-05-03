using ADAVoice.Core.Models;
using Microsoft.Extensions.Logging;

namespace ADAVoice.Core.Services;

/// <summary>
/// Service for tracking TTS usage and costs
/// </summary>
public class CostTrackingService : ICostTrackingService
{
    private readonly AppConfig _config;
    private readonly ILogger<CostTrackingService> _logger;
    private readonly string _usageDataPath;
    private CostInfo _currentCostInfo;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public CostTrackingService(AppConfig config, ILogger<CostTrackingService> logger)
    {
        _config = config;
        _logger = logger;
        _usageDataPath = config.UsageDataFile;
        _currentCostInfo = LoadUsageData();
    }
    
    public async Task AddUsageAsync(int characterCount)
    {
        if (!_config.EnableCostTracking)
        {
            return;
        }
        
        await _semaphore.WaitAsync();
        try
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            
            // Reset if it's a new month
            if (_currentCostInfo.CurrentMonth != currentMonth)
            {
                _currentCostInfo = new CostInfo
                {
                    CurrentMonth = currentMonth,
                    TotalCharacters = _currentCostInfo.TotalCharacters,
                    TotalCost = _currentCostInfo.TotalCost,
                    FreeTierLimit = _config.CostSettings.FreeTierCharacters,
                    CostPerCharacter = _config.CostSettings.CostPerCharacter,
                    DailyUsage = new Dictionary<string, long>()
                };
            }
            
            // Update daily usage
            if (!_currentCostInfo.DailyUsage.ContainsKey(today))
            {
                _currentCostInfo.DailyUsage[today] = 0;
            }
            _currentCostInfo.DailyUsage[today] += characterCount;
            
            // Update monthly usage
            _currentCostInfo.MonthlyCharacters += characterCount;
            
            // Update costs
            var cost = _config.CostSettings.CostPerCharacter * characterCount;
            _currentCostInfo.MonthlyCost += cost;
            _currentCostInfo.TotalCost += cost;
            _currentCostInfo.TotalCharacters += characterCount;
            
            // Check free tier warning
            if (_config.CostSettings.WarnOnFreeTierLimit)
            {
                var usagePercentage = (double)_currentCostInfo.MonthlyCharacters / _config.CostSettings.FreeTierCharacters;
                if (usagePercentage >= _config.CostSettings.FreeTierWarningThreshold)
                {
                    _logger.LogWarning($"Approaching free tier limit: {usagePercentage:P1} used ({_currentCostInfo.FreeTierRemaining:N0} characters remaining)");
                }
            }
            
            // Save updated data
            await SaveUsageDataAsync();
            
            _logger.LogDebug($"Added {characterCount:N0} characters to usage tracking");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<CostInfo> GetCostInfoAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _currentCostInfo.Clone();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<decimal> EstimateCostAsync(string text)
    {
        await Task.CompletedTask;
        return _config.CostSettings.CostPerCharacter * text.Length;
    }
    
    public async Task<bool> CanAffordAsync(string text, decimal budget)
    {
        var costInfo = await GetCostInfoAsync();
        var estimatedCost = await EstimateCostAsync(text);
        return (costInfo.MonthlyCost + estimatedCost) <= budget;
    }
    
    public async Task ResetUsageAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _currentCostInfo = new CostInfo
            {
                CurrentMonth = DateTime.Now.ToString("yyyy-MM"),
                FreeTierLimit = _config.CostSettings.FreeTierCharacters,
                CostPerCharacter = _config.CostSettings.CostPerCharacter
            };
            await SaveUsageDataAsync();
            _logger.LogInformation("Usage tracking reset");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private CostInfo LoadUsageData()
    {
        if (!File.Exists(_usageDataPath))
        {
            return new CostInfo
            {
                CurrentMonth = DateTime.Now.ToString("yyyy-MM"),
                FreeTierLimit = _config.CostSettings.FreeTierCharacters,
                CostPerCharacter = _config.CostSettings.CostPerCharacter
            };
        }
        
        try
        {
            var json = File.ReadAllText(_usageDataPath);
            var costInfo = System.Text.Json.JsonSerializer.Deserialize<CostInfo>(json);
            return costInfo ?? new CostInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load usage data, starting fresh");
            return new CostInfo();
        }
    }
    
    private async Task SaveUsageDataAsync()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_currentCostInfo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_usageDataPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save usage data");
        }
    }
    
    public void Dispose()
    {
        _semaphore.Dispose();
    }
}

/// <summary>
/// Interface for cost tracking services
/// </summary>
public interface ICostTrackingService : IDisposable
{
    /// <summary>
    /// Adds character usage to tracking
    /// </summary>
    Task AddUsageAsync(int characterCount);
    
    /// <summary>
    /// Gets current cost information
    /// </summary>
    Task<CostInfo> GetCostInfoAsync();
    
    /// <summary>
    /// Estimates cost for text
    /// </summary>
    Task<decimal> EstimateCostAsync(string text);
    
    /// <summary>
    /// Checks if text can be generated within budget
    /// </summary>
    Task<bool> CanAffordAsync(string text, decimal budget);
    
    /// <summary>
    /// Resets usage tracking
    /// </summary>
    Task ResetUsageAsync();
}
