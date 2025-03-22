namespace SeeSharpIndexer.Models
{
    /// <summary>
    /// Represents a relationship between types (inheritance, implementation, etc.)
    /// </summary>
    public class RelationshipModel
    {
        /// <summary>
        /// The type of relationship (inherits, implements, etc.)
        /// </summary>
        public string RelationshipType { get; set; } = string.Empty;

        /// <summary>
        /// The target type name in the relationship
        /// </summary>
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the relationship
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines the different types of relationships between classes
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// Class inheritance relationship (derived from)
        /// </summary>
        Inheritance,

        /// <summary>
        /// Interface implementation relationship
        /// </summary>
        Implementation,

        /// <summary>
        /// Composition relationship (has a member of type)
        /// </summary>
        Composition,

        /// <summary>
        /// Aggregation relationship (contains a collection of)
        /// </summary>
        Aggregation,

        /// <summary>
        /// Dependency relationship (uses as parameter or local variable)
        /// </summary>
        Dependency,

        /// <summary>
        /// Association relationship (references through method calls)
        /// </summary>
        Association
    }
} 