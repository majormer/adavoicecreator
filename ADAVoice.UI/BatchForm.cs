using ADAVoice.Core.Models;
using ADAVoice.Core.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Forms;

namespace ADAVoice.UI
{
    public partial class BatchForm : Form
    {
        private readonly ADAVoiceService _adaVoiceService;
        private readonly ILogger<BatchForm> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public BatchForm(ADAVoiceService adaVoiceService, ILogger<BatchForm> logger)
        {
            InitializeComponent();
            _adaVoiceService = adaVoiceService;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Batch Process - ADA Voice Creator";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(600, 400);

            // Create controls
            CreateInputSection();
            CreateSettingsSection();
            CreateProgressSection();
            CreateButtonSection();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateInputSection()
        {
            // Input GroupBox
            grpInput = new GroupBox
            {
                Text = "Input File",
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(660, 80)
            };

            // File label
            var lblFile = new Label
            {
                Text = "Select file with phrases (one per line):",
                Location = new System.Drawing.Point(6, 25),
                AutoSize = true
            };

            // File path text box
            txtInputFile = new TextBox
            {
                Location = new System.Drawing.Point(6, 45),
                Size = new System.Drawing.Size(520, 23)
            };

            // Browse button
            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(535, 45),
                Size = new System.Drawing.Size(75, 23)
            };

            grpInput.Controls.AddRange(new Control[] { lblFile, txtInputFile, btnBrowse });
            this.Controls.Add(grpInput);

            btnBrowse.Click += BtnBrowse_Click;
        }

        private void CreateSettingsSection()
        {
            // Settings GroupBox
            grpSettings = new GroupBox
            {
                Text = "Batch Settings",
                Location = new System.Drawing.Point(12, 100),
                Size = new System.Drawing.Size(660, 120)
            };

            // Output directory
            var lblDir = new Label
            {
                Text = "Output Directory:",
                Location = new System.Drawing.Point(6, 25),
                AutoSize = true
            };

            txtOutputDir = new TextBox
            {
                Text = "output",
                Location = new System.Drawing.Point(120, 20),
                Size = new System.Drawing.Size(400, 23)
            };

            btnBrowseDir = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(525, 20),
                Size = new System.Drawing.Size(75, 23)
            };

            // Format
            var lblFormat = new Label
            {
                Text = "Audio Format:",
                Location = new System.Drawing.Point(6, 55),
                AutoSize = true
            };

            cmbFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(120, 50),
                Size = new System.Drawing.Size(100, 23)
            };
            cmbFormat.Items.AddRange(new object[] { "MP3", "WAV", "OGG" });
            cmbFormat.SelectedIndex = 0;

            // Delay between requests
            var lblDelay = new Label
            {
                Text = "Delay (ms):",
                Location = new System.Drawing.Point(250, 55),
                AutoSize = true
            };

            numDelay = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 5000,
                Value = 100,
                Increment = 50,
                Location = new System.Drawing.Point(320, 50),
                Size = new System.Drawing.Size(80, 23)
            };

            // Stop on error checkbox
            chkStopOnError = new CheckBox
            {
                Text = "Stop on error",
                Checked = true,
                Location = new System.Drawing.Point(6, 85),
                AutoSize = true
            };

            grpSettings.Controls.AddRange(new Control[] {
                lblDir, txtOutputDir, btnBrowseDir,
                lblFormat, cmbFormat,
                lblDelay, numDelay,
                chkStopOnError
            });

            this.Controls.Add(grpSettings);

            btnBrowseDir.Click += BtnBrowseDir_Click;
        }

        private void CreateProgressSection()
        {
            // Progress GroupBox
            var grpProgress = new GroupBox
            {
                Text = "Progress",
                Location = new System.Drawing.Point(12, 230),
                Size = new System.Drawing.Size(660, 150)
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(6, 25),
                Size = new System.Drawing.Size(648, 23),
                Style = ProgressBarStyle.Continuous
            };

            // Status label
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new System.Drawing.Point(6, 55),
                AutoSize = true
            };

            // Progress details
            lblProgress = new Label
            {
                Text = "0 / 0 files processed",
                Location = new System.Drawing.Point(6, 75),
                AutoSize = true
            };

            // Cost label
            lblCost = new Label
            {
                Text = "Total Cost: $0.000000",
                Location = new System.Drawing.Point(6, 95),
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
            };

            // Results list
            lstResults = new ListBox
            {
                Location = new System.Drawing.Point(6, 115),
                Size = new System.Drawing.Size(648, 25),
                SelectionMode = SelectionMode.MultiSimple,
                ScrollAlwaysVisible = false
            };

            grpProgress.Controls.AddRange(new Control[] {
                progressBar, lblStatus, lblProgress, lblCost, lstResults
            });

            this.Controls.Add(grpProgress);
        }

        private void CreateButtonSection()
        {
            // Buttons
            btnStart = new Button
            {
                Text = "Start Batch",
                Location = new System.Drawing.Point(12, 390),
                Size = new System.Drawing.Size(120, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold)
            };

            btnStop = new Button
            {
                Text = "Stop",
                Location = new System.Drawing.Point(140, 390),
                Size = new System.Drawing.Size(120, 40),
                Enabled = false
            };

            btnOpenOutput = new Button
            {
                Text = "Open Output Folder",
                Location = new System.Drawing.Point(552, 390),
                Size = new System.Drawing.Size(120, 40)
            };

            this.Controls.AddRange(new Control[] { btnStart, btnStop, btnOpenOutput });

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnOpenOutput.Click += BtnOpenOutput_Click;
        }

        // Event Handlers
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt|All Files|*.*";
            openFileDialog.Title = "Select Input File";
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtInputFile.Text = openFileDialog.FileName;
            }
        }

        private void BtnBrowseDir_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select Output Directory";
            folderDialog.ShowNewFolderButton = true;
            
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtOutputDir.Text = folderDialog.SelectedPath;
            }
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputFile.Text) || !File.Exists(txtInputFile.Text))
            {
                MessageBox.Show("Please select a valid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                SetUIState(false);
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Read input file
                var lines = await File.ReadAllLinesAsync(txtInputFile.Text);
                var phrases = lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")).ToList();
                
                if (phrases.Count == 0)
                {
                    MessageBox.Show("No valid phrases found in input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize progress
                progressBar.Maximum = phrases.Count;
                progressBar.Value = 0;
                lblStatus.Text = "Processing...";
                lblProgress.Text = $"0 / {phrases.Count} files processed";
                lblCost.Text = "Total Cost: $0.000000";
                lstResults.Items.Clear();

                // Process batch
                var totalCost = 0m;
                var successCount = 0;
                var errorCount = 0;

                for (int i = 0; i < phrases.Count; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    var phrase = phrases[i];
                    lblProgress.Text = $"{i + 1} / {phrases.Count} files processed";
                    lblStatus.Text = $"Processing phrase {i + 1}...";

                    try
                    {
                        var request = new AudioRequest
                        {
                            Text = phrase,
                            Format = ParseFormat(cmbFormat.SelectedItem?.ToString() ?? "MP3"),
                            OutputDirectory = txtOutputDir.Text,
                            OutputPath = Path.Combine(txtOutputDir.Text, $"batch_{i + 1:D3}.{cmbFormat.SelectedItem?.ToString().ToLower()}")
                        };

                        var result = await _adaVoiceService.GenerateAudioAsync(request);
                        
                        if (result.IsSuccess)
                        {
                            totalCost += result.Cost;
                            successCount++;
                            lstResults.Items.Add($"✓ Phrase {i + 1}: {Path.GetFileName(result.OutputPath)}");
                        }
                        else
                        {
                            errorCount++;
                            lstResults.Items.Add($"✗ Phrase {i + 1}: {result.ErrorMessage}");
                            
                            if (chkStopOnError.Checked)
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        lstResults.Items.Add($"✗ Phrase {i + 1}: {ex.Message}");
                        _logger.LogError(ex, $"Error processing phrase {i + 1}");
                        
                        if (chkStopOnError.Checked)
                            break;
                    }

                    progressBar.Value = i + 1;
                    lblCost.Text = $"Total Cost: ${totalCost:F6}";

                    // Delay between requests
                    if (numDelay.Value > 0 && i < phrases.Count - 1)
                    {
                        await Task.Delay((int)numDelay.Value, _cancellationTokenSource.Token);
                    }
                }

                // Complete
                lblStatus.Text = $"Complete! {successCount} success, {errorCount} errors";
                
                if (errorCount == 0)
                {
                    MessageBox.Show($"Batch processing complete!\n\n{successCount} files generated\nTotal cost: ${totalCost:F6}", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Batch processing complete with errors!\n\n{successCount} files generated\n{errorCount} errors\nTotal cost: ${totalCost:F6}", 
                        "Complete with Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch processing");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            lblStatus.Text = "Cancelling...";
        }

        private void BtnOpenOutput_Click(object? sender, EventArgs e)
        {
            if (Directory.Exists(txtOutputDir.Text))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = txtOutputDir.Text,
                    UseShellExecute = true
                });
            }
        }

        // Helper Methods
        private void SetUIState(bool enabled)
        {
            btnStart.Enabled = enabled;
            btnStop.Enabled = !enabled;
            grpInput.Enabled = enabled;
            grpSettings.Enabled = enabled;
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

        // Control declarations
        private TextBox txtInputFile;
        private Button btnBrowse;
        private TextBox txtOutputDir;
        private Button btnBrowseDir;
        private ComboBox cmbFormat;
        private NumericUpDown numDelay;
        private CheckBox chkStopOnError;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblProgress;
        private Label lblCost;
        private ListBox lstResults;
        private Button btnStart;
        private Button btnStop;
        private Button btnOpenOutput;
        private GroupBox grpInput;
        private GroupBox grpSettings;
    }
}
