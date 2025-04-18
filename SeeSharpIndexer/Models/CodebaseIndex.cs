using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a complete codebase index
    /// </summary>
    public class CodebaseIndex
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Unnamed Codebase";

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("files")]
        public List<FileItem> Files { get; set; } = new List<FileItem>();

        [JsonProperty("totalFileCount")]
        public int TotalFileCount => Files.Count;

        [JsonProperty("totalClassCount")]
        public int TotalClassCount => Files.Sum(f => f.Classes.Count);

        [JsonProperty("totalInterfaceCount")]
        public int TotalInterfaceCount => Files.Sum(f => f.Interfaces.Count);

        [JsonProperty("totalEnumCount")]
        public int TotalEnumCount => Files.Sum(f => f.Enums.Count);

        /// <summary>
        /// Serializes the index to JSON
        /// </summary>
        public string ToJson(bool indented = false)
        {
            try
            {
                return JsonConvert.SerializeObject(this, 
                    indented ? Formatting.Indented : Formatting.None, 
                    new JsonSerializerSettings 
                    { 
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        Error = (sender, args) => args.ErrorContext.Handled = true
                    });
            }
            catch (Exception ex)
            {
                // In case of serialization error, return a valid JSON object with error information
                return JsonConvert.SerializeObject(new 
                { 
                    error = "Serialization error occurred", 
                    message = ex.Message,
                    name = this.Name,
                    timestamp = DateTime.Now
                }, Formatting.None);
            }
        }

        /// <summary>
        /// Deserializes JSON to a CodebaseIndex
        /// </summary>
        public static CodebaseIndex? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CodebaseIndex>(json);
        }

        /// <summary>
        /// Saves the index to a file
        /// </summary>
        public void SaveToFile(string filePath, bool minimize = true)
        {
            string json = ToJson(!minimize);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads an index from a file
        /// </summary>
        public static CodebaseIndex? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                
                // Validate JSON before parsing
                if (string.IsNullOrWhiteSpace(json) || !IsValidJson(json))
                {
                    throw new InvalidDataException("The file does not contain valid JSON data.");
                }
                
                return FromJson(json);
            }
            catch (Exception ex)
            {
                // Log or handle the error as appropriate
                System.Diagnostics.Debug.WriteLine($"Error loading index file: {ex.Message}");
                throw new InvalidDataException($"Failed to load index file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a string is valid JSON
        /// </summary>
        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
                return false;
                
            strInput = strInput.Trim();
            
            // Valid JSON must start with either an object or array
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || 
                (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    JsonConvert.DeserializeObject(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
            return false;
        }
    }
} 