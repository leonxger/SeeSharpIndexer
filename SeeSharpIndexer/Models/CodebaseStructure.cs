using System;
using System.Collections.Generic;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents the entire codebase structure
    /// </summary>
    public class CodebaseStructure
    {
        /// <summary>
        /// Root directory of the codebase
        /// </summary>
        public string RootDirectory { get; set; } = string.Empty;

        /// <summary>
        /// When the codebase was indexed
        /// </summary>
        public DateTime IndexingTimestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// All files in the codebase
        /// </summary>
        public List<FileNode> Files { get; set; } = new List<FileNode>();

        /// <summary>
        /// All namespaces found in the codebase
        /// </summary>
        public HashSet<string> AllNamespaces { get; set; } = new HashSet<string>();

        /// <summary>
        /// All classes found in the codebase
        /// </summary>
        public List<ClassModel> AllClasses { get; set; } = new List<ClassModel>();
    }

    /// <summary>
    /// Contains metadata about the indexed codebase
    /// </summary>
    public class CodebaseMetadata
    {
        /// <summary>
        /// Gets or sets the total number of files
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of classes
        /// </summary>
        public int ClassCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of methods
        /// </summary>
        public int MethodCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of properties
        /// </summary>
        public int PropertyCount { get; set; }

        /// <summary>
        /// Gets or sets stats about language distribution
        /// </summary>
        public Dictionary<string, int> LanguageDistribution { get; set; } = new Dictionary<string, int>();
    }
} 