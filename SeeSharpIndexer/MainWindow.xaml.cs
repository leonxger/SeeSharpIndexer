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
using SeeSharpIndexer.Core;
using SeeSharpIndexer.Models;

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

            try
            {
                // Show initial progress
                IndexingProgressBar.Value = 10;
                LogMessage("Scanning codebase...");
                
                // Create the required components
                var languageParser = new CSharpParser(); // Assuming this class exists
                var serializer = new CborSerializer();
                var tokenOptimizer = new TokenOptimizer(); // Assuming this class exists
                
                // Create the indexer
                var indexer = new SeeSharpIndexer.Core.CodebaseIndexer(
                    CodebasePathTextBox.Text,
                    languageParser,
                    serializer,
                    tokenOptimizer);
                
                // Set up the output file path
                string outputFilePath = Path.Combine(OutputPathTextBox.Text, "codebase_index.cbor");
                
                // Progress updates
                IndexingProgressBar.Value = 30;
                LogMessage("Analyzing C# files...");
                await Task.Delay(100, cancellationToken); // Small delay for UI update
                
                IndexingProgressBar.Value = 60;
                LogMessage("Processing codebase structure...");
                await Task.Delay(100, cancellationToken); // Small delay for UI update
                
                // Perform the actual indexing
                IndexingProgressBar.Value = 80;
                LogMessage("Serializing to CBOR format...");
                await indexer.IndexCodebaseAsync(outputFilePath);
                
                // Verify the file was created
                if (File.Exists(outputFilePath))
                {
                    IndexingProgressBar.Value = 100;
                    LogMessage($"Indexing completed successfully! Output saved to: {outputFilePath}", true);
                }
                else
                {
                    throw new Exception("Failed to create CBOR file. The file was not found after indexing.");
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