using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SeeSharpIndexer.Models;

namespace SeeSharpIndexer.Core
{
    /// <summary>
    /// Core class responsible for managing the codebase indexing process
    /// </summary>
    public class CodebaseIndexer
    {
        private readonly string _rootDirectory;
        private readonly ILanguageParser _languageParser;
        private readonly IIndexSerializer _indexSerializer;
        private readonly ITokenOptimizer _tokenOptimizer;

        public CodebaseIndexer(
            string rootDirectory, 
            ILanguageParser languageParser, 
            IIndexSerializer indexSerializer, 
            ITokenOptimizer tokenOptimizer)
        {
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            _languageParser = languageParser ?? throw new ArgumentNullException(nameof(languageParser));
            _indexSerializer = indexSerializer ?? throw new ArgumentNullException(nameof(indexSerializer));
            _tokenOptimizer = tokenOptimizer ?? throw new ArgumentNullException(nameof(tokenOptimizer));
        }

        /// <summary>
        /// Indexes the codebase starting from the root directory
        /// </summary>
        /// <param name="outputPath">Path where the index should be saved</param>
        /// <returns>Task representing the indexing operation</returns>
        public async Task IndexCodebaseAsync(string outputPath)
        {
            // 1. Scan for files in the codebase
            var files = ScanDirectory(_rootDirectory);

            // 2. Parse each file with the appropriate language parser
            var allClasses = new List<ClassModel>();
            foreach (var file in files)
            {
                if (Path.GetExtension(file).ToLowerInvariant() == ".cs" && _languageParser.CanParseFileExtension(".cs"))
                {
                    string sourceCode = await File.ReadAllTextAsync(file);
                    var classes = await _languageParser.ParseFileAsync(file, sourceCode);
                    if (classes != null && classes.Count > 0)
                    {
                        allClasses.AddRange(classes);
                    }
                }
            }

            // 3. Optimize the token structure to reduce size
            _tokenOptimizer.OptimizeClassModels(allClasses);

            // 4. Create codebase structure
            var codebaseStructure = new CodebaseStructure
            {
                AllClasses = allClasses,
                RootDirectory = _rootDirectory,
                IndexingTimestamp = DateTime.UtcNow
            };

            // 5. Serialize the data to the output format
            await _indexSerializer.SerializeAsync(codebaseStructure, outputPath);
        }

        private IEnumerable<string> ScanDirectory(string directory)
        {
            // Find all C# source files in the directory and subdirectories
            try
            {
                return Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to some directories: {ex.Message}");
                // Fall back to accessible directories only
                List<string> files = new List<string>();
                foreach (var dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        files.AddRange(ScanDirectory(dir));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip inaccessible directories
                    }
                }
                return files;
            }
        }
    }
} 