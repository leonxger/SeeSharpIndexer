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
        /// Whether to compress the output JSON file
        /// </summary>
        public bool CompressOutput
        {
            get => _settings.CompressOutput;
            set
            {
                if (_settings.CompressOutput != value)
                {
                    _settings.CompressOutput = value;
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
        /// Command to start the scanning process
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
        /// Command to clear all files
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

            // Subscribe to events
            _indexerService.FileProcessed += OnFileProcessed;
            _indexerService.ProgressUpdated += OnProgressUpdated;

            // Initialize commands
            AddFilesCommand = new RelayCommand(_ => AddFiles());
            AddDirectoryCommand = new RelayCommand(_ => AddDirectory());
            RemoveFilesCommand = new RelayCommand(_ => RemoveSelectedFiles(), _ => SelectedFilePath != null);
            ScanCommand = new RelayCommand(_ => ScanAsync().ConfigureAwait(false), _ => CanScan());
            SaveIndexCommand = new RelayCommand(_ => SaveIndex(), _ => _currentIndex != null);
            LoadIndexCommand = new RelayCommand(_ => LoadIndex());
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsOpen = !IsSettingsOpen);
            ClearFilesCommand = new RelayCommand(_ => ClearFiles(), _ => Files.Count > 0);
        }

        /// <summary>
        /// Add files to the project
        /// </summary>
        private void AddFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
                Multiselect = true,
                Title = "Select C# Files",
                InitialDirectory = !string.IsNullOrEmpty(_settings.LastOpenDirectory) 
                    ? _settings.LastOpenDirectory 
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                _settings.LastOpenDirectory = Path.GetDirectoryName(dialog.FileNames[0]) ?? string.Empty;
                _settings.Save();

                foreach (var filePath in dialog.FileNames)
                {
                    if (!Files.Any(f => f.FilePath == filePath))
                    {
                        var fileItem = new FileItem
                        {
                            Name = Path.GetFileName(filePath),
                            FilePath = filePath,
                            Location = filePath
                        };
                        Files.Add(fileItem);
                    }
                }

                StatusMessage = $"Added {dialog.FileNames.Length} file(s)";
            }
        }

        /// <summary>
        /// Add a directory to the project
        /// </summary>
        private void AddDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a directory containing C# files",
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrEmpty(_settings.LastOpenDirectory))
                dialog.InitialDirectory = _settings.LastOpenDirectory;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.LastOpenDirectory = dialog.SelectedPath;
                _settings.Save();

                try
                {
                    var files = _indexerService.GetCSharpFilesFromDirectory(dialog.SelectedPath, true);
                    int addedCount = 0;

                    foreach (var filePath in files)
                    {
                        if (!Files.Any(f => f.FilePath == filePath))
                        {
                            var fileItem = new FileItem
                            {
                                Name = Path.GetFileName(filePath),
                                FilePath = filePath,
                                Location = filePath
                            };
                            Files.Add(fileItem);
                            addedCount++;
                        }
                    }

                    StatusMessage = $"Added {addedCount} file(s) from directory";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    System.Windows.MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Remove selected files from the project
        /// </summary>
        private void RemoveSelectedFiles()
        {
            if (string.IsNullOrEmpty(SelectedFilePath))
                return;

            var itemsToRemove = Files.Where(f => f.FilePath == SelectedFilePath).ToList();
            foreach (var item in itemsToRemove)
            {
                Files.Remove(item);
            }

            StatusMessage = $"Removed {itemsToRemove.Count} file(s)";
        }

        /// <summary>
        /// Clear all files from the project
        /// </summary>
        private void ClearFiles()
        {
            Files.Clear();
            StatusMessage = "Cleared all files";
        }

        /// <summary>
        /// Scan the files and create an index
        /// </summary>
        private async Task ScanAsync()
        {
            if (IsScanning)
                return;

            IsScanning = true;
            ProgressValue = 0;
            StatusMessage = "Scanning...";

            try
            {
                var filePaths = Files.Where(f => f.IsSelected).Select(f => f.FilePath).ToList();
                _currentIndex = await _indexerService.CreateIndexAsync(filePaths, CodebaseName, CodebaseDescription);
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
            }
        }

        /// <summary>
        /// Save the index to a file
        /// </summary>
        private void SaveIndex()
        {
            if (_currentIndex == null)
                return;

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
                    // Set CompressOutput to false to ensure we save as plain JSON
                    bool originalCompressSetting = _settings.CompressOutput;
                    _settings.CompressOutput = false;
                    
                    _indexerService.SaveIndex(_currentIndex, dialog.FileName);
                    
                    // Restore original setting
                    _settings.CompressOutput = originalCompressSetting;
                    
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
                        Files.Add(file);
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
                StatusMessage = $"Processing: {Path.GetFileName(filePath)}";
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
        /// Determines if scanning can be started
        /// </summary>
        private bool CanScan()
        {
            return !IsScanning && Files.Count > 0 && Files.Any(f => f.IsSelected);
        }
    }
} 