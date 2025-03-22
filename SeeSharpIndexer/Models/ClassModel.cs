using System.Collections.Generic;

namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a class, interface, struct, or enum definition
    /// </summary>
    public class ClassModel
    {
        /// <summary>
        /// The name of the class
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The fully qualified name including namespace
        /// </summary>
        public string FullyQualifiedName { get; set; } = string.Empty;

        /// <summary>
        /// The namespace of the class
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Type of class (class, interface, struct, enum)
        /// </summary>
        public string ClassType { get; set; } = string.Empty;

        /// <summary>
        /// Access modifier (public, private, internal, protected)
        /// </summary>
        public string AccessModifier { get; set; } = string.Empty;

        /// <summary>
        /// Whether the class is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the class is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the class is sealed
        /// </summary>
        public bool IsSealed { get; set; }

        /// <summary>
        /// Whether the class is partial
        /// </summary>
        public bool IsPartial { get; set; }

        /// <summary>
        /// Whether the class is nested inside another class
        /// </summary>
        public bool IsNested { get; set; }

        /// <summary>
        /// If nested, the name of the parent class
        /// </summary>
        public string ParentClassName { get; set; } = string.Empty;

        /// <summary>
        /// Documentation comments
        /// </summary>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// Methods defined in this class
        /// </summary>
        public List<MethodModel> Methods { get; set; } = new List<MethodModel>();

        /// <summary>
        /// Properties defined in this class
        /// </summary>
        public List<PropertyModel> Properties { get; set; } = new List<PropertyModel>();

        /// <summary>
        /// Relationships this class has with other types (inheritance, implementation, etc.)
        /// </summary>
        public List<RelationshipModel> Relationships { get; set; } = new List<RelationshipModel>();
    }
} 