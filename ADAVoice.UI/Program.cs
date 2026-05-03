using ADAVoice.Core.Services;
using ADAVoice.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ADAVoice.Core;

namespace ADAVoice.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Set up dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            using var serviceProvider = services.BuildServiceProvider();
            
            // Validate configuration before starting
            var configService = serviceProvider.GetRequiredService<ConfigurationService>();
            var config = configService.LoadConfiguration();
            
            // Check if credentials are configured
            if (string.IsNullOrEmpty(config.GoogleCloudCredentialsPath))
            {
                MessageBox.Show(
                    "Google Cloud credentials are not configured.\n\n" +
                    "Please copy .env.sample to .env and add your credentials file path.\n\n" +
                    "The application will continue in demo mode without TTS functionality.",
                    "Configuration Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            
            // Run the main form
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start ADA Voice Creator:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton<AppConfig>(provider =>
        {
            var configService = new ConfigurationService(
                provider.GetRequiredService<ILogger<ConfigurationService>>());
            return configService.LoadConfiguration();
        });
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Services
        services.AddSingleton<ConfigurationService>();
        
        // Try to initialize Google Cloud TTS service, fall back to null service if credentials invalid
        services.AddSingleton<ITTSService>(sp => {
            try {
                var config = sp.GetRequiredService<AppConfig>();
                var logger = sp.GetRequiredService<ILogger<GoogleCloudTTSService>>();
                return new GoogleCloudTTSService(config, logger);
            } catch {
                // Return null service if credentials not configured
                return sp.GetRequiredService<NullTSService>();
            }
        });
        
        services.AddSingleton<GoogleCloudTTSService>(sp => sp.GetRequiredService<ITTSService>() as GoogleCloudTTSService);
        services.AddSingleton<NullTSService>();
        services.AddSingleton<CostTrackingService>();
        services.AddSingleton<ICostTrackingService>(provider => provider.GetRequiredService<CostTrackingService>());
        services.AddSingleton<AudioConversionService>();
        services.AddSingleton<ADAVoiceService>();
        
        // Forms
        services.AddSingleton<MainForm>();
    }
}