using SeeSharpIndexer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SeeSharpIndexer.Services
{
    /// <summary>
    /// Service for scanning and indexing C# code files
    /// </summary>
    public class IndexerService
    {
        private readonly CodeParser _codeParser;
        private readonly IndexerSettings _settings;

        public event EventHandler<string>? FileProcessed;
        public event EventHandler<int>? ProgressUpdated;

        public IndexerService(IndexerSettings settings)
        {
            _settings = settings;
            _codeParser = new CodeParser(settings);
        }

        /// <summary>
        /// Scan all C# files in the given paths and create an index
        /// </summary>
        public async Task<CodebaseIndex> CreateIndexAsync(IEnumerable<string> filePaths, string codeName = "", string description = "")
        {
            var index = new CodebaseIndex
            {
                Name = string.IsNullOrWhiteSpace(codeName) ? "Unnamed Codebase" : codeName,
                Description = description,
                CreatedAt = DateTime.Now
            };

            // Filter for C# files only
            var csharpFiles = filePaths
                .Where(path => Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalFiles = csharpFiles.Count;
            int processedFiles = 0;

            foreach (var filePath in csharpFiles)
            {
                try
                {
                    var fileItem = await _codeParser.ParseFileAsync(filePath);
                    index.Files.Add(fileItem);
                    
                    // Raise events for progress tracking
                    FileProcessed?.Invoke(this, filePath);
                    processedFiles++;
                    ProgressUpdated?.Invoke(this, (int)((double)processedFiles / totalFiles * 100));
                }
                catch (Exception ex)
                {
                    // Log error but continue with other files
                    System.Diagnostics.Debug.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }

            return index;
        }

        /// <summary>
        /// Scan a directory recursively for C# files
        /// </summary>
        public IEnumerable<string> GetCSharpFilesFromDirectory(string directoryPath, bool recursive = true)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directoryPath, "*.cs", searchOption);
        }

        /// <summary>
        /// Save the index to a file
        /// </summary>
        public void SaveIndex(CodebaseIndex index, string filePath)
        {
            index.SaveToFile(filePath, _settings.MinimizeJson);
        }

        /// <summary>
        /// Load an index from a file
        /// </summary>
        public CodebaseIndex? LoadIndex(string filePath)
        {
            return CodebaseIndex.LoadFromFile(filePath);
        }
    }
} 