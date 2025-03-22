using System.Collections.Generic;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a method in a class
    /// </summary>
    public class MethodModel
    {
        /// <summary>
        /// The name of the method
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The return type of the method
        /// </summary>
        public string ReturnType { get; set; } = string.Empty;

        /// <summary>
        /// The access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = string.Empty;

        /// <summary>
        /// Whether the method is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the method is virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Whether the method overrides a base method
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Whether the method is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the method is asynchronous
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// The starting line number in the source file
        /// </summary>
        public int StartLineNumber { get; set; }

        /// <summary>
        /// The ending line number in the source file
        /// </summary>
        public int EndLineNumber { get; set; }

        /// <summary>
        /// Documentation comments
        /// </summary>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// The method parameters
        /// </summary>
        public List<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();
    }

    /// <summary>
    /// Represents a method parameter
    /// </summary>
    public class ParameterModel
    {
        /// <summary>
        /// The parameter name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The parameter type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Whether the parameter has a default value
        /// </summary>
        public bool HasDefaultValue { get; set; }

        /// <summary>
        /// The default value as a string, if any
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Whether the parameter is passed by reference
        /// </summary>
        public bool IsByRef { get; set; }

        /// <summary>
        /// Whether the parameter is an out parameter
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// Whether the parameter is optional
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Whether the parameter is params array
        /// </summary>
        public bool IsParams { get; set; }
    }
} 