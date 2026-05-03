using ADAVoice.Core.Models;
using ADAVoice.Core.Services;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ADAVoice.UI
{
    public partial class MainForm : Form
    {
        private readonly ADAVoiceService _adaVoiceService;
        private readonly ILogger<MainForm> _logger;
        private readonly string _outputDirectory = "output";

        // Controls
        private GroupBox grpInput = null!;
        private TextBox txtInput = null!;
        private Label lblCharCount = null!;
        private GroupBox grpVoice = null!;
        private TrackBar trackRate = null!;
        private TrackBar trackPitch = null!;
        private TrackBar trackVolume = null!;
        private Label lblRateValue = null!;
        private Label lblPitchValue = null!;
        private Label lblVolumeValue = null!;
        private ComboBox cmbVoice = null!;
        private GroupBox grpOutput = null!;
        private ComboBox cmbFormat = null!;
        private TextBox txtOutputFile = null!;
        private TextBox txtOutputDir = null!;
        private Button btnBrowse = null!;
        private Button btnBrowseDir = null!;
        private CheckBox chkAutoFilename = null!;
        private Button btnGenerate = null!;
        private Button btnEstimate = null!;
        private Button btnBatch = null!;
        private Button btnSettings = null!;
        private GroupBox grpStatus = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private Label lblCostInfo = null!;
        private Label lblLastOutput = null!;

        public MainForm(ADAVoiceService adaVoiceService, ILogger<MainForm> logger)
        {
            try
            {
                _adaVoiceService = adaVoiceService;
                _logger = logger;
                
                InitializeComponent();
                InitializeForm();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing MainForm");
                MessageBox.Show($"Error initializing the main form:\n\n{ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "ADA Voice Creator";
            this.Size = new Size(850, 720);
            this.MinimumSize = new Size(750, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateInputSection();
            CreateVoiceSettingsSection();
            CreateOutputSection();
            CreateActionSection();
            CreateStatusSection();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateInputSection()
        {
            grpInput = new GroupBox
            {
                Text = "Text Input",
                Location = new Point(12, 12),
                Size = new Size(810, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblText = new Label
            {
                Text = "Enter text to convert to ADA voice:",
                Location = new Point(10, 22),
                AutoSize = true
            };

            txtInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 42),
                Size = new Size(790, 105),
                Font = new Font("Segoe UI", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblCharCount = new Label
            {
                Text = "Characters: 0",
                Location = new Point(10, 152),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            grpInput.Controls.AddRange(new Control[] { lblText, txtInput, lblCharCount });
            this.Controls.Add(grpInput);

            txtInput.TextChanged += (s, e) => UpdateCharacterCount();
        }

        private void CreateVoiceSettingsSection()
        {
            grpVoice = new GroupBox
            {
                Text = "Voice Settings",
                Location = new Point(12, 200),
                Size = new Size(400, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            int y = 22;
            int labelX = 10;
            int controlX = 115;
            int valueX = 330;
            int rowHeight = 28;

            // Speaking Rate
            var lblRate = new Label { Text = "Speaking Rate:", Location = new Point(labelX, y + 3), AutoSize = true };
            trackRate = new TrackBar
            {
                Minimum = 25, Maximum = 400, Value = 95,
                Location = new Point(controlX, y), Size = new Size(200, 25),
                TickStyle = TickStyle.None, AutoSize = false
            };
            lblRateValue = new Label { Text = "0.95", Location = new Point(valueX, y + 3), AutoSize = true };
            trackRate.ValueChanged += (s, e) => lblRateValue.Text = (trackRate.Value / 100.0).ToString("F2");

            y += rowHeight;

            // Pitch
            var lblPitch = new Label { Text = "Pitch:", Location = new Point(labelX, y + 3), AutoSize = true };
            trackPitch = new TrackBar
            {
                Minimum = -200, Maximum = 200, Value = 0,
                Location = new Point(controlX, y), Size = new Size(200, 25),
                TickStyle = TickStyle.None, AutoSize = false
            };
            lblPitchValue = new Label { Text = "0.0", Location = new Point(valueX, y + 3), AutoSize = true };
            trackPitch.ValueChanged += (s, e) => lblPitchValue.Text = (trackPitch.Value / 10.0).ToString("F1");

            y += rowHeight;

            // Volume
            var lblVolume = new Label { Text = "Volume (dB):", Location = new Point(labelX, y + 3), AutoSize = true };
            trackVolume = new TrackBar
            {
                Minimum = -96, Maximum = 16, Value = 0,
                Location = new Point(controlX, y), Size = new Size(200, 25),
                TickStyle = TickStyle.None, AutoSize = false
            };
            lblVolumeValue = new Label { Text = "0.0", Location = new Point(valueX, y + 3), AutoSize = true };
            trackVolume.ValueChanged += (s, e) => lblVolumeValue.Text = trackVolume.Value.ToString("F1");

            y += rowHeight + 5;

            // Voice
            var lblVoice = new Label { Text = "Voice:", Location = new Point(labelX, y + 3), AutoSize = true };
            cmbVoice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(controlX, y),
                Size = new Size(220, 25)
            };
            cmbVoice.Items.AddRange(new object[] {
                "en-US-Wavenet-C (Female)",
                "en-US-Wavenet-D (Male)",
                "en-US-Wavenet-E (Female)",
                "en-US-Wavenet-F (Female)",
                "en-US-Wavenet-A (Male)",
                "en-US-Wavenet-B (Male)"
            });
            cmbVoice.SelectedIndex = 0;

            grpVoice.Controls.AddRange(new Control[] {
                lblRate, trackRate, lblRateValue,
                lblPitch, trackPitch, lblPitchValue,
                lblVolume, trackVolume, lblVolumeValue,
                lblVoice, cmbVoice
            });

            this.Controls.Add(grpVoice);
        }

        private void CreateOutputSection()
        {
            grpOutput = new GroupBox
            {
                Text = "Output Settings",
                Location = new Point(420, 200),
                Size = new Size(402, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int y = 25;
            int labelX = 10;
            int controlX = 95;
            int btnX = 350;
            int rowHeight = 35;

            // Format
            var lblFormat = new Label { Text = "Format:", Location = new Point(labelX, y + 3), AutoSize = true };
            cmbFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(controlX, y),
                Size = new Size(100, 25)
            };
            cmbFormat.Items.AddRange(new object[] { "MP3", "WAV", "OGG" });
            cmbFormat.SelectedIndex = 0;

            y += rowHeight;

            // Output File
            var lblOutput = new Label { Text = "Output File:", Location = new Point(labelX, y + 3), AutoSize = true };
            txtOutputFile = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(245, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnBrowse = new Button
            {
                Text = "...",
                Location = new Point(btnX, y),
                Size = new Size(35, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowse.Click += BtnBrowse_Click;

            y += rowHeight;

            // Output Directory
            var lblDir = new Label { Text = "Output Dir:", Location = new Point(labelX, y + 3), AutoSize = true };
            txtOutputDir = new TextBox
            {
                Text = _outputDirectory,
                Location = new Point(controlX, y),
                Size = new Size(235, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnBrowseDir = new Button
            {
                Text = "...",
                Location = new Point(btnX, y),
                Size = new Size(45, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowseDir.Click += BtnBrowseDir_Click;

            y += rowHeight;

            // Auto-generate filename
            chkAutoFilename = new CheckBox
            {
                Text = "Auto-generate filename",
                Checked = true,
                Location = new Point(controlX, y),
                AutoSize = true
            };
            chkAutoFilename.CheckedChanged += ChkAutoFilename_CheckedChanged;

            grpOutput.Controls.AddRange(new Control[] {
                lblFormat, cmbFormat,
                lblOutput, txtOutputFile, btnBrowse,
                lblDir, txtOutputDir, btnBrowseDir,
                chkAutoFilename
            });

            this.Controls.Add(grpOutput);

            // Initialize state
            ChkAutoFilename_CheckedChanged(null, EventArgs.Empty);
        }

        private void CreateActionSection()
        {
            int y = 410;

            btnGenerate = new Button
            {
                Text = "Generate",
                Location = new Point(12, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnEstimate = new Button
            {
                Text = "Estimate",
                Location = new Point(120, y),
                Size = new Size(100, 35)
            };
            btnEstimate.Click += BtnEstimate_Click;

            btnBatch = new Button
            {
                Text = "Batch",
                Location = new Point(230, y),
                Size = new Size(100, 35)
            };
            btnBatch.Click += BtnBatch_Click;

            btnSettings = new Button
            {
                Text = "Settings",
                Location = new Point(722, y),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSettings.Click += BtnSettings_Click;

            this.Controls.AddRange(new Control[] { btnGenerate, btnEstimate, btnBatch, btnSettings });
        }

        private void CreateStatusSection()
        {
            grpStatus = new GroupBox
            {
                Text = "Status",
                Location = new Point(12, 455),
                Size = new Size(810, 195),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(10, 22),
                AutoSize = true
            };

            progressBar = new ProgressBar
            {
                Location = new Point(10, 45),
                Size = new Size(790, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblCostInfo = new Label
            {
                Text = "Cost: $0.000000",
                Location = new Point(10, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblLastOutput = new Label
            {
                Text = "Last output: None",
                Location = new Point(10, 105),
                Size = new Size(790, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            grpStatus.Controls.AddRange(new Control[] { lblStatus, progressBar, lblCostInfo, lblLastOutput });
            this.Controls.Add(grpStatus);
        }

        private void InitializeForm()
        {
            UpdateCharacterCount();
        }

        private void UpdateCharacterCount()
        {
            lblCharCount.Text = $"Characters: {txtInput.Text.Length}";
        }

        // Event Handlers
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "MP3 Files|*.mp3|WAV Files|*.wav|OGG Files|*.ogg|All Files|*.*",
                Title = "Save Audio File"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                txtOutputFile.Text = dialog.FileName;
        }

        private void BtnBrowseDir_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Output Directory",
                ShowNewFolderButton = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                txtOutputDir.Text = dialog.SelectedPath;
        }

        private void ChkAutoFilename_CheckedChanged(object? sender, EventArgs e)
        {
            txtOutputFile.Enabled = !chkAutoFilename.Checked;
            btnBrowse.Enabled = !chkAutoFilename.Checked;
            if (chkAutoFilename.Checked)
                txtOutputFile.Text = "";
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Please enter text to convert.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                SetUIState(false);
                lblStatus.Text = "Generating audio...";
                progressBar.Value = 25;

                var voiceSettings = new VoiceSettings
                {
                    LanguageCode = "en-US",
                    VoiceName = ExtractVoiceName(cmbVoice.SelectedItem?.ToString() ?? "en-US-Wavenet-C"),
                    SpeakingRate = trackRate.Value / 100.0,
                    Pitch = trackPitch.Value / 10.0,
                    VolumeGainDb = trackVolume.Value
                };

                var request = new AudioRequest
                {
                    Text = txtInput.Text,
                    Format = ParseFormat(cmbFormat.SelectedItem?.ToString() ?? "MP3"),
                    OutputDirectory = txtOutputDir.Text,
                    OutputPath = chkAutoFilename.Checked ? null : txtOutputFile.Text,
                    VoiceSettings = voiceSettings
                };

                progressBar.Value = 50;
                var result = await _adaVoiceService.GenerateAudioAsync(request);
                progressBar.Value = 100;

                if (result.IsSuccess)
                {
                    lblStatus.Text = "Success!";
                    lblCostInfo.Text = $"Cost: ${result.Cost:F6}";
                    lblLastOutput.Text = $"Last output: {result.OutputPath}";

                    if (MessageBox.Show("Audio generated successfully! Play the file?", "Success",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = result.OutputPath,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    lblStatus.Text = $"Error: {result.ErrorMessage}";
                    MessageBox.Show(result.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio");
                lblStatus.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Error generating audio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
                progressBar.Value = 0;
            }
        }

        private async void BtnEstimate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Please enter text to estimate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                lblStatus.Text = "Estimating cost...";
                var cost = await _adaVoiceService.EstimateCostAsync(txtInput.Text);
                lblCostInfo.Text = $"Estimated Cost: ${cost:F6}";
                lblStatus.Text = "Ready";
                MessageBox.Show($"Estimated cost: ${cost:F6}\nCharacters: {txtInput.Text.Length:N0}",
                    "Cost Estimate", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating cost");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBatch_Click(object? sender, EventArgs e)
        {
            var batchForm = new BatchForm(_adaVoiceService,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<BatchForm>.Instance);
            batchForm.ShowDialog();
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_adaVoiceService,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<SettingsForm>.Instance);
            settingsForm.ShowDialog();
        }

        private void SetUIState(bool enabled)
        {
            btnGenerate.Enabled = enabled;
            btnEstimate.Enabled = enabled;
            btnBatch.Enabled = enabled;
            grpInput.Enabled = enabled;
            grpVoice.Enabled = enabled;
            grpOutput.Enabled = enabled;
        }

        private string ExtractVoiceName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "en-US-Wavenet-C";
            var parts = displayName.Split(' ');
            return parts.Length > 0 ? parts[0] : "en-US-Wavenet-C";
        }

        private AudioFormat ParseFormat(string format)
        {
            return format?.ToUpperInvariant() switch
            {
                "WAV" => AudioFormat.Wav,
                "OGG" => AudioFormat.Ogg,
                _ => AudioFormat.Mp3
            };
        }
    }
}
