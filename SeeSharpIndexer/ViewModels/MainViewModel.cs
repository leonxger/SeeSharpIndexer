using Microsoft.Win32;
using SeeSharpIndexer.Helpers;
using SeeSharpIndexer.Models;
using SeeSharpIndexer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SeeSharpIndexer.ViewModels
{
    /// <summary>
    /// View model for the main window
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IndexerService _indexerService;
        private readonly IndexerSettings _settings;

        private string _statusMessage = "Ready";
        private int _progressValue = 0;
        private bool _isScanning = false;
        private bool _isSettingsOpen = false;
        private string _codebaseName = string.Empty;
        private string _codebaseDescription = string.Empty;
        private CodebaseIndex? _currentIndex;
        private string? _selectedFilePath;
        private string _currentProcessingFileName = string.Empty;

        #region Properties

        /// <summary>
        /// List of files to be processed
        /// </summary>
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();

        /// <summary>
        /// The selected file path
        /// </summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set => SetProperty(ref _selectedFilePath, value);
        }

        /// <summary>
        /// The name of the codebase
        /// </summary>
        public string CodebaseName
        {
            get => _codebaseName;
            set => SetProperty(ref _codebaseName, value);
        }

        /// <summary>
        /// The description of the codebase
        /// </summary>
        public string CodebaseDescription
        {
            get => _codebaseDescription;
            set => SetProperty(ref _codebaseDescription, value);
        }

        /// <summary>
        /// The current status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// The current file being processed
        /// </summary>
        public string CurrentProcessingFileName
        {
            get => _currentProcessingFileName;
            set => SetProperty(ref _currentProcessingFileName, value);
        }

        /// <summary>
        /// The current progress value (0-100)
        /// </summary>
        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        /// <summary>
        /// Whether scanning is in progress
        /// </summary>
        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        /// <summary>
        /// Whether the settings panel is open
        /// </summary>
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set => SetProperty(ref _isSettingsOpen, value);
        }

        /// <summary>
        /// Whether to include private members in the index
        /// </summary>
        public bool IncludePrivateMembers
        {
            get => _settings.IncludePrivateMembers;
            set
            {
                if (_settings.IncludePrivateMembers != value)
                {
                    _settings.IncludePrivateMembers = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to include internal members in the index
        /// </summary>
        public bool IncludeInternalMembers
        {
            get => _settings.IncludeInternalMembers;
            set
            {
                if (_settings.IncludeInternalMembers != value)
                {
                    _settings.IncludeInternalMembers = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to include protected members in the index
        /// </summary>
        public bool IncludeProtectedMembers
        {
            get => _settings.IncludeProtectedMembers;
            set
            {
                if (_settings.IncludeProtectedMembers != value)
                {
                    _settings.IncludeProtectedMembers = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to include fields in the index
        /// </summary>
        public bool IncludeFields
        {
            get => _settings.IncludeFields;
            set
            {
                if (_settings.IncludeFields != value)
                {
                    _settings.IncludeFields = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to minimize the output JSON file to a single line
        /// </summary>
        public bool MinimizeJson
        {
            get => _settings.MinimizeJson;
            set
            {
                if (_settings.MinimizeJson != value)
                {
                    _settings.MinimizeJson = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to add files to the project
        /// </summary>
        public ICommand AddFilesCommand { get; }

        /// <summary>
        /// Command to add a directory to the project
        /// </summary>
        public ICommand AddDirectoryCommand { get; }

        /// <summary>
        /// Command to remove selected files from the project
        /// </summary>
        public ICommand RemoveFilesCommand { get; }

        /// <summary>
        /// Command to scan the files and create an index
        /// </summary>
        public ICommand ScanCommand { get; }

        /// <summary>
        /// Command to save the index to a file
        /// </summary>
        public ICommand SaveIndexCommand { get; }

        /// <summary>
        /// Command to load an index from a file
        /// </summary>
        public ICommand LoadIndexCommand { get; }

        /// <summary>
        /// Command to toggle the settings panel
        /// </summary>
        public ICommand ToggleSettingsCommand { get; }

        /// <summary>
        /// Command to clear all files from the project
        /// </summary>
        public ICommand ClearFilesCommand { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            _settings = IndexerSettings.Load();
            _indexerService = new IndexerService(_settings);

            _indexerService.FileProcessed += OnFileProcessed;
            _indexerService.ProgressUpdated += OnProgressUpdated;

            // Set up commands
            AddFilesCommand = new RelayCommand(AddFiles);
            AddDirectoryCommand = new RelayCommand(AddDirectory);
            RemoveFilesCommand = new RelayCommand(RemoveSelectedFiles, () => SelectedFilePath != null);
            ScanCommand = new RelayCommand(async () => await ScanAsync(), () => CanScan());
            SaveIndexCommand = new RelayCommand(SaveIndex, () => _currentIndex != null);
            LoadIndexCommand = new RelayCommand(LoadIndex);
            ToggleSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
            ClearFilesCommand = new RelayCommand(ClearFiles, () => Files.Count > 0);

            // Ensure commands are always accessible
            System.Windows.Application.Current.Dispatcher.ShutdownStarted += (s, e) => {
                _indexerService.FileProcessed -= OnFileProcessed;
                _indexerService.ProgressUpdated -= OnProgressUpdated;
            };
        }

        /// <summary>
        /// Add files to the project
        /// </summary>
        private void AddFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
                Title = "Select C# Files",
                Multiselect = true,
                InitialDirectory = !string.IsNullOrEmpty(_settings.LastOpenDirectory)
                    ? _settings.LastOpenDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                _settings.LastOpenDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                _settings.Save();

                var filePaths = dialog.FileNames;
                var addedCount = 0;

                foreach (var filePath in filePaths)
                {
                    if (Files.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    Files.Add(new FileItem(filePath, true));
                    addedCount++;
                }

                StatusMessage = $"Added {addedCount} file(s).";
            }
        }

        /// <summary>
        /// Add a directory to the project
        /// </summary>
        private void AddDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder containing C# files",
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrEmpty(_settings.LastOpenDirectory) && Directory.Exists(_settings.LastOpenDirectory))
            {
                dialog.InitialDirectory = _settings.LastOpenDirectory;
            }
            else
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.LastOpenDirectory = dialog.SelectedPath;
                _settings.Save();

                var filePaths = Directory.GetFiles(dialog.SelectedPath, "*.cs", SearchOption.AllDirectories);
                var addedCount = 0;

                foreach (var filePath in filePaths)
                {
                    if (Files.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    Files.Add(new FileItem(filePath, true));
                    addedCount++;
                }

                StatusMessage = $"Added {addedCount} file(s) from directory.";
            }
        }

        /// <summary>
        /// Remove selected files from the project
        /// </summary>
        private void RemoveSelectedFiles()
        {
            if (string.IsNullOrEmpty(SelectedFilePath))
                return;

            var selectedItems = Files.Where(f => f.FilePath == SelectedFilePath).ToList();
            foreach (var item in selectedItems)
            {
                Files.Remove(item);
            }

            SelectedFilePath = null;
            StatusMessage = "Removed selected file(s).";
        }

        /// <summary>
        /// Clear all files from the project
        /// </summary>
        private void ClearFiles()
        {
            Files.Clear();
            SelectedFilePath = null;
            StatusMessage = "All files cleared.";
        }

        /// <summary>
        /// Scan the files and create an index
        /// </summary>
        private async Task ScanAsync()
        {
            if (IsScanning)
                return;

            // Prepare UI for scanning
            IsScanning = true;
            ProgressValue = 0;
            StatusMessage = "Preparing to scan files...";
            CurrentProcessingFileName = string.Empty;

            try
            {
                var filePaths = Files.Where(f => f.IsSelected).Select(f => f.FilePath).ToList();
                
                if (filePaths.Count == 0)
                {
                    throw new InvalidOperationException("No files selected for scanning. Please select at least one file.");
                }

                StatusMessage = "Scanning files...";
                _currentIndex = await _indexerService.CreateIndexAsync(filePaths, CodebaseName, CodebaseDescription);
                
                // Make sure all files in the index have IsSelected = true
                foreach (var file in _currentIndex.Files)
                {
                    file.IsSelected = true;
                }
                
                StatusMessage = $"Scanning complete. Processed {_currentIndex.Files.Count} files.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                System.Windows.MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsScanning = false;
                CurrentProcessingFileName = string.Empty;
            }
        }

        /// <summary>
        /// Save the index to a file
        /// </summary>
        private void SaveIndex()
        {
            if (_currentIndex == null)
            {
                System.Windows.MessageBox.Show("No index available to save. Please scan files first.", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            // Update the index with current name and description
            _currentIndex.Name = CodebaseName;
            _currentIndex.Description = CodebaseDescription;
            
            // Filter out files that are not selected
            _currentIndex.Files = _currentIndex.Files.Where(f => f.IsSelected).ToList();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Save Index File",
                InitialDirectory = !string.IsNullOrEmpty(_settings.LastSaveDirectory)
                    ? _settings.LastSaveDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FileName = !string.IsNullOrEmpty(CodebaseName) 
                    ? $"{CodebaseName.Replace(" ", "_")}_index.json" 
                    : "codebase_index.json"
            };

            if (dialog.ShowDialog() == true)
            {
                _settings.LastSaveDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                _settings.Save();

                try
                {
                    // Don't override the user's setting
                    _indexerService.SaveIndex(_currentIndex, dialog.FileName);
                    StatusMessage = $"Index saved to {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    System.Windows.MessageBox.Show($"Failed to save index: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Load an index from a file
        /// </summary>
        private void LoadIndex()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Open Index File",
                InitialDirectory = !string.IsNullOrEmpty(_settings.LastSaveDirectory)
                    ? _settings.LastSaveDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                _settings.LastSaveDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                _settings.Save();

                try
                {
                    // First verify that the file is valid
                    if (!File.Exists(dialog.FileName))
                    {
                        throw new FileNotFoundException($"File '{dialog.FileName}' not found.");
                    }

                    if (new FileInfo(dialog.FileName).Length == 0)
                    {
                        throw new InvalidDataException("The file is empty.");
                    }

                    var loadedIndex = _indexerService.LoadIndex(dialog.FileName);
                    
                    if (loadedIndex == null)
                    {
                        throw new InvalidDataException("Failed to deserialize the index file. The file may be corrupt or in an incompatible format.");
                    }

                    _currentIndex = loadedIndex;

                    // Update UI
                    CodebaseName = _currentIndex.Name;
                    CodebaseDescription = _currentIndex.Description;
                    Files.Clear();
                    
                    foreach (var file in _currentIndex.Files)
                    {
                        // Create a new FileItem with the IsSelected property from the loaded file
                        var newFileItem = new FileItem(file.FilePath, file.IsSelected)
                        {
                            Classes = file.Classes,
                            Enums = file.Enums,
                            Interfaces = file.Interfaces
                        };
                        Files.Add(newFileItem);
                    }

                    StatusMessage = $"Index loaded from {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    System.Windows.MessageBox.Show($"Failed to load index: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Called when a file is processed
        /// </summary>
        private void OnFileProcessed(object? sender, string filePath)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentProcessingFileName = Path.GetFileName(filePath);
                StatusMessage = $"Processing: {CurrentProcessingFileName}";
            });
        }

        /// <summary>
        /// Called when progress is updated
        /// </summary>
        private void OnProgressUpdated(object? sender, int progress)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = progress;
            });
        }

        /// <summary>
        /// Returns whether the scan command can be executed
        /// </summary>
        private bool CanScan()
        {
            return !IsScanning && Files.Any(f => f.IsSelected);
        }
    }
} 