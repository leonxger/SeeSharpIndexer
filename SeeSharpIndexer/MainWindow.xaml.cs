using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace SeeSharpIndexer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isIndexing = false;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set default output path
            OutputPathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SeeSharpIndexer");
        }

        private void BrowseCodebaseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select C# Codebase Directory";
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CodebasePathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Output Directory";
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OutputPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isIndexing)
                return;

            if (string.IsNullOrWhiteSpace(CodebasePathTextBox.Text))
            {
                LogMessage("Please select a codebase directory first.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
            {
                LogMessage("Please select an output directory first.", true);
                return;
            }

            if (!Directory.Exists(CodebasePathTextBox.Text))
            {
                LogMessage("Selected codebase directory does not exist.", true);
                return;
            }

            try
            {
                // Create output directory if it doesn't exist
                if (!Directory.Exists(OutputPathTextBox.Text))
                {
                    Directory.CreateDirectory(OutputPathTextBox.Text);
                }

                // Set UI state to indexing
                _isIndexing = true;
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                BrowseCodebaseButton.IsEnabled = false;
                BrowseOutputButton.IsEnabled = false;
                
                // Clear log and reset progress
                LogTextBox.Clear();
                IndexingProgressBar.Value = 0;
                
                // Start indexing process
                _cancellationTokenSource = new CancellationTokenSource();
                await StartIndexingAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}", true);
            }
            finally
            {
                // Reset UI state
                _isIndexing = false;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                BrowseCodebaseButton.IsEnabled = true;
                BrowseOutputButton.IsEnabled = true;
                _cancellationTokenSource = null;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            LogMessage("Indexing cancelled by user.", true);
        }

        private async Task StartIndexingAsync(CancellationToken cancellationToken)
        {
            LogMessage("Starting indexing process...");
            LogMessage($"Codebase: {CodebasePathTextBox.Text}");
            LogMessage($"Output: {OutputPathTextBox.Text}");

            // TODO: Connect to the actual indexing functionality once implemented
            // For now, just simulate progress
            
            try
            {
                for (int i = 0; i <= 100; i += 5)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Update progress bar
                    IndexingProgressBar.Value = i;
                    
                    if (i == 0)
                        LogMessage("Scanning codebase...");
                    else if (i == 20)
                        LogMessage("Analyzing C# files...");
                    else if (i == 40)
                        LogMessage("Extracting class metadata...");
                    else if (i == 60)
                        LogMessage("Processing relationships...");
                    else if (i == 80)
                        LogMessage("Optimizing tokens...");
                    else if (i == 100)
                        LogMessage("Serializing to CBOR format...");

                    // Simulate processing time
                    await Task.Delay(300, cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    string outputFilePath = Path.Combine(OutputPathTextBox.Text, "codebase_index.cbor");
                    LogMessage($"Indexing completed successfully! Output saved to: {outputFilePath}", true);
                }
            }
            catch (OperationCanceledException)
            {
                LogMessage("Indexing operation was cancelled.", true);
            }
            catch (Exception ex)
            {
                LogMessage($"Error during indexing: {ex.Message}", true);
            }
        }

        private void LogMessage(string message, bool isImportant = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
            
            LogTextBox.AppendText(logEntry);
            LogTextBox.ScrollToEnd();
            
            if (isImportant)
            {
                System.Windows.MessageBox.Show(message, "SeeSharpIndexer", MessageBoxButton.OK, 
                    isImportant ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }
    }
}