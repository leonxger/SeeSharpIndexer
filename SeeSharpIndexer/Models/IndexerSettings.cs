using Newtonsoft.Json;
using System;
using System.IO;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Settings for the code indexer application
    /// </summary>
    public class IndexerSettings
    {
        public static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SeeSharpIndexer",
            "settings.json");

        #region Scanning Options

        /// <summary>
        /// Whether to include private members in the index
        /// </summary>
        public bool IncludePrivateMembers { get; set; } = false;

        /// <summary>
        /// Whether to include internal members in the index
        /// </summary>
        public bool IncludeInternalMembers { get; set; } = true;

        /// <summary>
        /// Whether to include protected members in the index
        /// </summary>
        public bool IncludeProtectedMembers { get; set; } = true;

        /// <summary>
        /// Whether to include fields in the index
        /// </summary>
        public bool IncludeFields { get; set; } = true;

        /// <summary>
        /// Whether to include XML documentation comments
        /// </summary>
        public bool IncludeXmlComments { get; set; } = false;

        /// <summary>
        /// Whether to include attributes in the index
        /// </summary>
        public bool IncludeAttributes { get; set; } = false;

        #endregion

        #region Compression Options

        /// <summary>
        /// Whether to compress the output JSON file
        /// </summary>
        public bool CompressOutput { get; set; } = true;

        /// <summary>
        /// Whether to indent the JSON for readability (only applies when compression is disabled)
        /// </summary>
        public bool IndentJson { get; set; } = true;

        #endregion

        #region UI Settings

        /// <summary>
        /// The last directory used for opening files
        /// </summary>
        public string LastOpenDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The last directory used for saving files
        /// </summary>
        public string LastSaveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The list of recent projects
        /// </summary>
        public string[] RecentProjects { get; set; } = Array.Empty<string>();

        #endregion

        /// <summary>
        /// Save settings to the application data directory
        /// </summary>
        public void Save()
        {
            string directory = Path.GetDirectoryName(SettingsFilePath) ?? string.Empty;
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }

        /// <summary>
        /// Load settings from the application data directory
        /// </summary>
        public static IndexerSettings Load()
        {
            if (!File.Exists(SettingsFilePath))
                return new IndexerSettings();

            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<IndexerSettings>(json) ?? new IndexerSettings();
            }
            catch
            {
                return new IndexerSettings();
            }
        }
    }
} 