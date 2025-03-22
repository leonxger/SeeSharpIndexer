using System;
using System.Collections.Generic;
using System.IO;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a source code file in the codebase
    /// </summary>
    public class FileNode
    {
        /// <summary>
        /// Full path to the file
        /// </summary>
        public string AbsolutePath { get; set; } = string.Empty;

        /// <summary>
        /// Path relative to the codebase root
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// The programming language of the file
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// When the file was last modified
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Namespaces declared in the file
        /// </summary>
        public List<string> Namespaces { get; set; } = new List<string>();

        /// <summary>
        /// Import statements in the file
        /// </summary>
        public List<string> Imports { get; set; } = new List<string>();

        /// <summary>
        /// Classes, interfaces, enums, and structs defined in the file
        /// </summary>
        public List<ClassModel> Classes { get; set; } = new List<ClassModel>();

        /// <summary>
        /// Path property used by TokenOptimizer
        /// </summary>
        public string Path => RelativePath ?? AbsolutePath;

        /// <summary>
        /// Gets the file name with extension
        /// </summary>
        public string FileName => System.IO.Path.GetFileName(AbsolutePath);

        /// <summary>
        /// Gets the file extension
        /// </summary>
        public string Extension => System.IO.Path.GetExtension(AbsolutePath);
    }
} 