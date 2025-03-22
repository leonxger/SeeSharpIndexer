using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Base class for all code items that will be indexed
    /// </summary>
    public class CodeItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("accessibility")]
        public string Accessibility { get; set; } = "public";

        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsSelected { get; set; } = true;
    }

    /// <summary>
    /// Represents a C# file in the codebase
    /// </summary>
    public class FileItem : CodeItem
    {
        public FileItem()
        {
            Type = "File";
        }

        [JsonProperty("classes")]
        public List<ClassItem> Classes { get; set; } = new List<ClassItem>();

        [JsonProperty("enums")]
        public List<EnumItem> Enums { get; set; } = new List<EnumItem>();

        [JsonProperty("interfaces")]
        public List<InterfaceItem> Interfaces { get; set; } = new List<InterfaceItem>();

        [JsonProperty("filePath")]
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a class in the codebase
    /// </summary>
    public class ClassItem : CodeItem
    {
        public ClassItem()
        {
            Type = "Class";
        }

        [JsonProperty("methods")]
        public List<MethodItem> Methods { get; set; } = new List<MethodItem>();

        [JsonProperty("properties")]
        public List<PropertyItem> Properties { get; set; } = new List<PropertyItem>();

        [JsonProperty("fields")]
        public List<FieldItem> Fields { get; set; } = new List<FieldItem>();

        [JsonProperty("baseTypes")]
        public List<string> BaseTypes { get; set; } = new List<string>();

        [JsonProperty("isStatic")]
        public bool IsStatic { get; set; } = false;

        [JsonProperty("isAbstract")]
        public bool IsAbstract { get; set; } = false;
    }

    /// <summary>
    /// Represents an interface in the codebase
    /// </summary>
    public class InterfaceItem : CodeItem
    {
        public InterfaceItem()
        {
            Type = "Interface";
        }

        [JsonProperty("methods")]
        public List<MethodItem> Methods { get; set; } = new List<MethodItem>();

        [JsonProperty("properties")]
        public List<PropertyItem> Properties { get; set; } = new List<PropertyItem>();

        [JsonProperty("baseInterfaces")]
        public List<string> BaseInterfaces { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents an enum in the codebase
    /// </summary>
    public class EnumItem : CodeItem
    {
        public EnumItem()
        {
            Type = "Enum";
        }

        [JsonProperty("values")]
        public List<string> Values { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a method in the codebase
    /// </summary>
    public class MethodItem : CodeItem
    {
        public MethodItem()
        {
            Type = "Method";
        }

        [JsonProperty("returnType")]
        public string ReturnType { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public List<ParameterItem> Parameters { get; set; } = new List<ParameterItem>();

        [JsonProperty("isStatic")]
        public bool IsStatic { get; set; } = false;

        [JsonProperty("isAbstract")]
        public bool IsAbstract { get; set; } = false;

        [JsonProperty("isVirtual")]
        public bool IsVirtual { get; set; } = false;
    }

    /// <summary>
    /// Represents a property in the codebase
    /// </summary>
    public class PropertyItem : CodeItem
    {
        public PropertyItem()
        {
            Type = "Property";
        }

        [JsonProperty("propertyType")]
        public string PropertyType { get; set; } = string.Empty;

        [JsonProperty("hasGetter")]
        public bool HasGetter { get; set; } = true;

        [JsonProperty("hasSetter")]
        public bool HasSetter { get; set; } = true;

        [JsonProperty("isStatic")]
        public bool IsStatic { get; set; } = false;
    }

    /// <summary>
    /// Represents a field in the codebase
    /// </summary>
    public class FieldItem : CodeItem
    {
        public FieldItem()
        {
            Type = "Field";
        }

        [JsonProperty("fieldType")]
        public string FieldType { get; set; } = string.Empty;

        [JsonProperty("isStatic")]
        public bool IsStatic { get; set; } = false;

        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; } = false;
    }

    /// <summary>
    /// Represents a method parameter
    /// </summary>
    public class ParameterItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("defaultValue", NullValueHandling = NullValueHandling.Ignore)]
        public string? DefaultValue { get; set; }
    }
} 