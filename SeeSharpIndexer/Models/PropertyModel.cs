namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a property in a class
    /// </summary>
    public class PropertyModel
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the property
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = string.Empty;

        /// <summary>
        /// Whether the property is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the property is virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Whether the property overrides a base property
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Whether the property is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the property has a getter
        /// </summary>
        public bool HasGetter { get; set; } = true;

        /// <summary>
        /// Whether the property has a setter
        /// </summary>
        public bool HasSetter { get; set; } = true;

        /// <summary>
        /// Whether the setter is private
        /// </summary>
        public bool HasPrivateSetter { get; set; }

        /// <summary>
        /// Whether the property is auto-implemented
        /// </summary>
        public bool IsAutoImplemented { get; set; }

        /// <summary>
        /// Documentation comments
        /// </summary>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line number where the property begins in the source file
        /// </summary>
        public int StartLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the line number where the property ends in the source file
        /// </summary>
        public int EndLineNumber { get; set; }
    }
} 