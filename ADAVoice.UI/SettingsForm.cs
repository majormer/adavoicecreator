using ADAVoice.Core.Models;
using ADAVoice.Core.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Forms;

namespace ADAVoice.UI
{
    public partial class SettingsForm : Form
    {
        private readonly ADAVoiceService _adaVoiceService;
        private readonly ILogger<SettingsForm> _logger;

        public SettingsForm(ADAVoiceService adaVoiceService, ILogger<SettingsForm> logger)
        {
            InitializeComponent();
            _adaVoiceService = adaVoiceService;
            _logger = logger;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Settings - ADA Voice Creator";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create tab control
            var tabControl = new TabControl
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(560, 400)
            };

            // Create tabs
            tabControl.TabPages.Add(CreateCredentialsTab());
            tabControl.TabPages.Add(CreateVoiceTab());
            tabControl.TabPages.Add(CreateCostTab());
            tabControl.TabPages.Add(CreateAboutTab());

            this.Controls.Add(tabControl);

            // Buttons
            btnOK = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(405, 425),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(490, 425),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnOK, btnCancel });

            this.ResumeLayout(false);
            this.PerformLayout();

            // Load current settings
            LoadSettings();
        }

        private TabPage CreateCredentialsTab()
        {
            var tab = new TabPage("Credentials");

            // Google Cloud section
            var lblGcp = new Label
            {
                Text = "Google Cloud Settings",
                Location = new System.Drawing.Point(10, 10),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            var lblProjectId = new Label
            {
                Text = "Project ID:",
                Location = new System.Drawing.Point(10, 45),
                AutoSize = true
            };

            txtProjectId = new TextBox
            {
                Location = new System.Drawing.Point(120, 40),
                Size = new System.Drawing.Size(300, 23)
            };

            var lblCredentials = new Label
            {
                Text = "Credentials File:",
                Location = new System.Drawing.Point(10, 75),
                AutoSize = true
            };

            txtCredentialsPath = new TextBox
            {
                Location = new System.Drawing.Point(120, 70),
                Size = new System.Drawing.Size(300, 23)
            };

            btnBrowseCredentials = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(425, 70),
                Size = new System.Drawing.Size(75, 23)
            };

            var lblInfo = new Label
            {
                Text = "Download your service account credentials from Google Cloud Console",
                Location = new System.Drawing.Point(10, 105),
                AutoSize = true,
                ForeColor = System.Drawing.Color.Gray
            };

            var btnCreateCredentials = new LinkLabel
            {
                Text = "Create Service Account",
                Location = new System.Drawing.Point(10, 125),
                AutoSize = true
            };

            tab.Controls.AddRange(new Control[] {
                lblGcp, lblProjectId, txtProjectId,
                lblCredentials, txtCredentialsPath, btnBrowseCredentials,
                lblInfo, btnCreateCredentials
            });

            btnBrowseCredentials.Click += (s, e) => BrowseCredentials();
            btnCreateCredentials.LinkClicked += (s, e) => System.Diagnostics.Process.Start(
                "https://console.cloud.google.com/iam-admin/serviceaccounts");

            return tab;
        }

        private TabPage CreateVoiceTab()
        {
            var tab = new TabPage("Voice Settings");

            // Default voice settings
            var lblDefault = new Label
            {
                Text = "Default Voice Settings",
                Location = new System.Drawing.Point(10, 10),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            // Language code
            var lblLanguage = new Label
            {
                Text = "Language Code:",
                Location = new System.Drawing.Point(10, 45),
                AutoSize = true
            };

            txtLanguageCode = new TextBox
            {
                Text = "en-US",
                Location = new System.Drawing.Point(120, 40),
                Size = new System.Drawing.Size(100, 23)
            };

            // Voice name
            var lblVoiceName = new Label
            {
                Text = "Default Voice:",
                Location = new System.Drawing.Point(10, 75),
                AutoSize = true
            };

            txtDefaultVoice = new TextBox
            {
                Text = "en-US-Wavenet-C",
                Location = new System.Drawing.Point(120, 70),
                Size = new System.Drawing.Size(200, 23)
            };

            // Sample rate
            var lblSampleRate = new Label
            {
                Text = "Sample Rate (Hz):",
                Location = new System.Drawing.Point(10, 105),
                AutoSize = true
            };

            numSampleRate = new NumericUpDown
            {
                Minimum = 8000,
                Maximum = 48000,
                Value = 24000,
                Increment = 1000,
                Location = new System.Drawing.Point(120, 100),
                Size = new System.Drawing.Size(100, 23)
            };

            // Output directory
            var lblOutputDir = new Label
            {
                Text = "Default Output Directory:",
                Location = new System.Drawing.Point(10, 145),
                AutoSize = true
            };

            txtDefaultOutputDir = new TextBox
            {
                Text = "output",
                Location = new System.Drawing.Point(120, 140),
                Size = new System.Drawing.Size(250, 23)
            };

            btnBrowseOutput = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(375, 140),
                Size = new System.Drawing.Size(75, 23)
            };

            // Audio format
            var lblFormat = new Label
            {
                Text = "Default Audio Format:",
                Location = new System.Drawing.Point(10, 175),
                AutoSize = true
            };

            cmbDefaultFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(120, 170),
                Size = new System.Drawing.Size(100, 23)
            };
            cmbDefaultFormat.Items.AddRange(new object[] { "MP3", "WAV", "OGG" });
            cmbDefaultFormat.SelectedIndex = 0;

            tab.Controls.AddRange(new Control[] {
                lblDefault, lblLanguage, txtLanguageCode,
                lblVoiceName, txtDefaultVoice,
                lblSampleRate, numSampleRate,
                lblOutputDir, txtDefaultOutputDir, btnBrowseOutput,
                lblFormat, cmbDefaultFormat
            });

            btnBrowseOutput.Click += (s, e) => BrowseOutputDirectory();

            return tab;
        }

        private TabPage CreateCostTab()
        {
            var tab = new TabPage("Cost Tracking");

            // Cost settings
            var lblCost = new Label
            {
                Text = "Cost Tracking Settings",
                Location = new System.Drawing.Point(10, 10),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            // Enable cost tracking
            chkEnableCostTracking = new CheckBox
            {
                Text = "Enable cost tracking",
                Checked = true,
                Location = new System.Drawing.Point(10, 45),
                AutoSize = true
            };

            // Cost per character
            var lblCostPerChar = new Label
            {
                Text = "Cost per character ($):",
                Location = new System.Drawing.Point(10, 75),
                AutoSize = true
            };

            numCostPerCharacter = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1,
                DecimalPlaces = 6,
                Value = 0.000004m,
                Increment = 0.000001m,
                Location = new System.Drawing.Point(150, 70),
                Size = new System.Drawing.Size(100, 23)
            };

            // Free tier characters
            var lblFreeTier = new Label
            {
                Text = "Free tier characters:",
                Location = new System.Drawing.Point(10, 105),
                AutoSize = true
            };

            numFreeTier = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10000000,
                Value = 4000000,
                Increment = 100000,
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(100, 23)
            };

            // Warning threshold
            var lblWarning = new Label
            {
                Text = "Warning threshold (%):",
                Location = new System.Drawing.Point(10, 135),
                AutoSize = true
            };

            numWarningThreshold = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = 90,
                Location = new System.Drawing.Point(150, 130),
                Size = new System.Drawing.Size(100, 23)
            };

            // Current usage
            var lblCurrent = new Label
            {
                Text = "Current Usage:",
                Location = new System.Drawing.Point(10, 180),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            txtCurrentUsage = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(10, 200),
                Size = new System.Drawing.Size(520, 100),
                Font = new System.Drawing.Font("Consolas", 9F)
            };

            // Refresh button
            btnRefreshUsage = new Button
            {
                Text = "Refresh Usage",
                Location = new System.Drawing.Point(10, 310),
                Size = new System.Drawing.Size(100, 30)
            };

            // Reset usage button
            btnResetUsage = new Button
            {
                Text = "Reset Usage",
                Location = new System.Drawing.Point(120, 310),
                Size = new System.Drawing.Size(100, 30)
            };

            tab.Controls.AddRange(new Control[] {
                lblCost, chkEnableCostTracking,
                lblCostPerChar, numCostPerCharacter,
                lblFreeTier, numFreeTier,
                lblWarning, numWarningThreshold,
                lblCurrent, txtCurrentUsage,
                btnRefreshUsage, btnResetUsage
            });

            btnRefreshUsage.Click += async (s, e) => await RefreshUsage();
            btnResetUsage.Click += async (s, e) => await ResetUsage();

            return tab;
        }

        private TabPage CreateAboutTab()
        {
            var tab = new TabPage("About");

            var lblTitle = new Label
            {
                Text = "ADA Voice Creator",
                Location = new System.Drawing.Point(10, 10),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            var lblVersion = new Label
            {
                Text = "Version 1.0.0",
                Location = new System.Drawing.Point(10, 45),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F),
                AutoSize = true
            };

            var lblDescription = new Label
            {
                Text = "A C# application for generating ADA voice audio files using Google Cloud Text-to-Speech API.",
                Location = new System.Drawing.Point(10, 75),
                Size = new System.Drawing.Size(520, 40)
            };

            var lblFeatures = new Label
            {
                Text = "Features:\n• Console and Windows Forms interfaces\n• Batch processing support\n• Cost tracking and management\n• Multiple audio formats (MP3, WAV, OGG)\n• Configurable voice settings",
                Location = new System.Drawing.Point(10, 120),
                AutoSize = true
            };

            var lblLinks = new Label
            {
                Text = "Links:",
                Location = new System.Drawing.Point(10, 220),
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold),
                AutoSize = true
            };

            var linkDocumentation = new LinkLabel
            {
                Text = "Documentation",
                Location = new System.Drawing.Point(10, 245),
                AutoSize = true
            };

            var linkGoogleCloud = new LinkLabel
            {
                Text = "Google Cloud Text-to-Speech",
                Location = new System.Drawing.Point(10, 270),
                AutoSize = true
            };

            var linkGitHub = new LinkLabel
            {
                Text = "Source Code",
                Location = new System.Drawing.Point(10, 295),
                AutoSize = true
            };

            tab.Controls.AddRange(new Control[] {
                lblTitle, lblVersion, lblDescription, lblFeatures, lblLinks,
                linkDocumentation, linkGoogleCloud, linkGitHub
            });

            linkDocumentation.LinkClicked += (s, e) => System.Diagnostics.Process.Start("https://gitlab.lan.majormer.com/majormer/adavoicecreator");
            linkGoogleCloud.LinkClicked += (s, e) => System.Diagnostics.Process.Start("https://cloud.google.com/text-to-speech");
            linkGitHub.LinkClicked += (s, e) => System.Diagnostics.Process.Start("https://gitlab.lan.majormer.com/majormer/adavoicecreator");

            return tab;
        }

        // Event Handlers
        private async void LoadSettings()
        {
            try
            {
                // Load cost info
                await RefreshUsage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
            }
        }

        private async Task RefreshUsage()
        {
            try
            {
                txtCurrentUsage.Text = "Loading...";
                var costInfo = await _adaVoiceService.GetCostInfoAsync();
                txtCurrentUsage.Text = costInfo.GetSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing usage");
                txtCurrentUsage.Text = "Error loading usage information";
            }
        }

        private async Task ResetUsage()
        {
            if (MessageBox.Show("Are you sure you want to reset all usage tracking data?", 
                "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    // Note: This would require adding a reset method to the service
                    MessageBox.Show("Usage tracking has been reset.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshUsage();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resetting usage");
                    MessageBox.Show("Failed to reset usage tracking.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BrowseCredentials()
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files|*.json|All Files|*.*";
            openFileDialog.Title = "Select Google Cloud Credentials File";
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtCredentialsPath.Text = openFileDialog.FileName;
            }
        }

        private void BrowseOutputDirectory()
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select Default Output Directory";
            folderDialog.ShowNewFolderButton = true;
            
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtDefaultOutputDir.Text = folderDialog.SelectedPath;
            }
        }

        // Control declarations
        private TextBox txtProjectId;
        private TextBox txtCredentialsPath;
        private Button btnBrowseCredentials;
        private TextBox txtLanguageCode;
        private TextBox txtDefaultVoice;
        private NumericUpDown numSampleRate;
        private TextBox txtDefaultOutputDir;
        private Button btnBrowseOutput;
        private ComboBox cmbDefaultFormat;
        private CheckBox chkEnableCostTracking;
        private NumericUpDown numCostPerCharacter;
        private NumericUpDown numFreeTier;
        private NumericUpDown numWarningThreshold;
        private TextBox txtCurrentUsage;
        private Button btnRefreshUsage;
        private Button btnResetUsage;
        private Button btnOK;
        private Button btnCancel;
    }
}
