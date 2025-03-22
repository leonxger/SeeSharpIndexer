using System;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Describes the schema of the index file
    /// </summary>
    public class IndexSchema
    {
        /// <summary>
        /// Version of the schema
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Format of the index (json, cbor, etc.)
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Whether the index data is compressed
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Compression method, if applicable
        /// </summary>
        public string CompressionMethod { get; set; } = string.Empty;

        /// <summary>
        /// Description of the index format
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tool that generated this index
        /// </summary>
        public string Generator { get; set; } = "SeeSharpIndexer";

        /// <summary>
        /// Gets or sets the timestamp when the index was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the codebase structure
        /// </summary>
        public CodebaseStructure Codebase { get; set; } = new CodebaseStructure();

        /// <summary>
        /// Gets or sets a map of common strings used in the index to reduce duplication
        /// </summary>
        public StringMap StringMap { get; set; } = new StringMap();

        /// <summary>
        /// Gets or sets metadata about the indexing process
        /// </summary>
        public IndexingMetadata Metadata { get; set; } = new IndexingMetadata();
    }

    /// <summary>
    /// Contains metadata about the indexing process
    /// </summary>
    public class IndexingMetadata
    {
        /// <summary>
        /// Gets or sets the duration of the indexing process in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Gets or sets the total number of files processed
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens generated
        /// </summary>
        public int TokenCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the index in bytes
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the compression ratio achieved (original/compressed)
        /// </summary>
        public double CompressionRatio { get; set; }
    }

    /// <summary>
    /// Map of common strings used in the index to reduce duplication
    /// </summary>
    public class StringMap
    {
        /// <summary>
        /// Gets or sets the map of common types
        /// </summary>
        public string[] Types { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the map of common namespaces
        /// </summary>
        public string[] Namespaces { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the map of common modifiers
        /// </summary>
        public string[] Modifiers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the map of common method names
        /// </summary>
        public string[] MethodNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the map of common property names
        /// </summary>
        public string[] PropertyNames { get; set; } = Array.Empty<string>();
    }
} 