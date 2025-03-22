using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SeeSharpIndexer.Models;

namespace SeeSharpIndexer.Core
{
    /// <summary>
    /// Optimizes and compresses extracted code metadata to reduce output size
    /// </summary>
    public class TokenOptimizer : ITokenOptimizer
    {
        private Dictionary<string, int> _stringIdMap = new Dictionary<string, int>();
        private int _nextStringId = 1;

        /// <summary>
        /// Optimizes class models by compressing metadata to reduce output size
        /// </summary>
        /// <param name="classModels">The list of class models to optimize</param>
        public void OptimizeClassModels(List<ClassModel> classModels)
        {
            if (classModels == null)
            {
                throw new ArgumentNullException(nameof(classModels));
            }

            // Reset string ID map for new optimization session
            _stringIdMap.Clear();
            _nextStringId = 1;

            // Process and modify each class model in place
            foreach (var classModel in classModels)
            {
                // Optimize documentation strings
                if (!string.IsNullOrWhiteSpace(classModel.Documentation))
                {
                    classModel.Documentation = OptimizeDocumentation(classModel.Documentation);
                }

                // Optimize method documentation and signatures
                foreach (var method in classModel.Methods)
                {
                    if (!string.IsNullOrWhiteSpace(method.Documentation))
                    {
                        method.Documentation = OptimizeDocumentation(method.Documentation);
                    }
                }

                // Optimize property documentation
                foreach (var property in classModel.Properties)
                {
                    if (!string.IsNullOrWhiteSpace(property.Documentation))
                    {
                        property.Documentation = OptimizeDocumentation(property.Documentation);
                    }
                }
            }
        }

        /// <summary>
        /// Optimizes extracted code tokens to reduce output size
        /// </summary>
        public object OptimizeTokens(IEnumerable<object> parsedData)
        {
            if (parsedData == null)
            {
                throw new ArgumentNullException(nameof(parsedData));
            }

            // Reset string ID map for new optimization session
            _stringIdMap.Clear();
            _nextStringId = 1;

            // Process the data
            var processedData = ProcessData(parsedData);
            
            return new
            {
                OptimizationLevel = "Advanced",
                Timestamp = DateTime.UtcNow,
                FileCount = processedData.Count(),
                StringTable = _stringIdMap.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToList(),
                Files = processedData
            };
        }

        /// <summary>
        /// Optimizes documentation by removing redundant whitespace and truncating if needed
        /// </summary>
        private string OptimizeDocumentation(string documentation)
        {
            if (string.IsNullOrWhiteSpace(documentation))
                return string.Empty;
            
            // Remove excess whitespace
            var trimmed = Regex.Replace(documentation, @"\s+", " ").Trim();
            
            // Truncate very long documentation (over 200 chars)
            if (trimmed.Length > 200)
            {
                trimmed = trimmed.Substring(0, 197) + "...";
            }
            
            return trimmed;
        }

        private IEnumerable<object> ProcessData(IEnumerable<object> parsedData)
        {
            var result = new List<object>();
            
            foreach (var item in parsedData)
            {
                if (item is FileNode fileNode)
                {
                    result.Add(ProcessFileNode(fileNode));
                }
                else
                {
                    // Handle other types if needed
                    result.Add(item);
                }
            }
            
            return result;
        }

        private object ProcessFileNode(FileNode fileNode)
        {
            // Make a copy to avoid modifying the original
            var optimizedClasses = new List<object>();
            
            foreach (var classModel in fileNode.Classes)
            {
                optimizedClasses.Add(ProcessClassModel(classModel));
            }
            
            return new
            {
                Path = GetOrCreateStringId(fileNode.Path),
                Language = GetOrCreateStringId(fileNode.Language),
                Classes = optimizedClasses
            };
        }

        private object ProcessClassModel(ClassModel classModel)
        {
            var optimizedMethods = new List<object>();
            var optimizedProperties = new List<object>();
            var optimizedRelationships = new List<object>();
            
            foreach (var method in classModel.Methods)
            {
                optimizedMethods.Add(ProcessMethodModel(method));
            }
            
            foreach (var property in classModel.Properties)
            {
                optimizedProperties.Add(ProcessPropertyModel(property));
            }
            
            foreach (var relationship in classModel.Relationships)
            {
                optimizedRelationships.Add(ProcessRelationshipModel(relationship));
            }

            return new
            {
                Name = GetOrCreateStringId(classModel.Name),
                FQN = GetOrCreateStringId(classModel.FullyQualifiedName),
                Namespace = GetOrCreateStringId(classModel.Namespace),
                AccessMod = GetOrCreateStringId(classModel.AccessModifier),
                Type = GetOrCreateStringId(classModel.ClassType),
                IsStatic = classModel.IsStatic,
                IsAbstract = classModel.IsAbstract,
                IsSealed = classModel.IsSealed,
                IsPartial = classModel.IsPartial,
                IsNested = classModel.IsNested,
                ParentClass = GetOrCreateStringId(classModel.ParentClassName),
                Docs = SummarizeComment(classModel.Documentation),
                Methods = optimizedMethods,
                Properties = optimizedProperties,
                Relationships = optimizedRelationships
            };
        }

        private object ProcessMethodModel(MethodModel methodModel)
        {
            var optimizedParameters = new List<object>();
            
            foreach (var param in methodModel.Parameters)
            {
                optimizedParameters.Add(ProcessParameterModel(param));
            }

            return new
            {
                Name = GetOrCreateStringId(methodModel.Name),
                RetType = GetOrCreateStringId(methodModel.ReturnType),
                AccessMod = GetOrCreateStringId(methodModel.AccessModifier),
                IsStatic = methodModel.IsStatic,
                IsAbstract = methodModel.IsAbstract,
                IsVirtual = methodModel.IsVirtual,
                IsOverride = methodModel.IsOverride,
                IsAsync = methodModel.IsAsync,
                Signature = CompressMethodSignature(methodModel),
                Docs = SummarizeComment(methodModel.Documentation),
                Params = optimizedParameters,
                Start = methodModel.StartLineNumber,
                End = methodModel.EndLineNumber
            };
        }

        private object ProcessParameterModel(Models.ParameterModel parameterModel)
        {
            return new
            {
                Name = GetOrCreateStringId(parameterModel.Name),
                Type = GetOrCreateStringId(parameterModel.Type),
                IsOpt = parameterModel.IsOptional,
                Default = GetOrCreateStringId(parameterModel.DefaultValue),
                IsByRef = parameterModel.IsByRef,
                IsOut = parameterModel.IsOut,
                IsParams = parameterModel.IsParams
            };
        }

        private object ProcessPropertyModel(PropertyModel propertyModel)
        {
            return new
            {
                Name = GetOrCreateStringId(propertyModel.Name),
                Type = GetOrCreateStringId(propertyModel.Type),
                AccessMod = GetOrCreateStringId(propertyModel.AccessModifier),
                IsStatic = propertyModel.IsStatic,
                IsVirtual = propertyModel.IsVirtual,
                IsOverride = propertyModel.IsOverride,
                HasGetter = propertyModel.HasGetter,
                HasSetter = propertyModel.HasSetter,
                Docs = SummarizeComment(propertyModel.Documentation)
            };
        }

        private object ProcessRelationshipModel(RelationshipModel relationshipModel)
        {
            return new
            {
                Type = GetOrCreateStringId(relationshipModel.RelationshipType),
                Target = GetOrCreateStringId(relationshipModel.TargetType)
            };
        }

        #region Token Optimization Techniques

        /// <summary>
        /// Implements basic comment summarization by removing redundant whitespace and truncating long comments
        /// </summary>
        private int SummarizeComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return 0; // Use 0 to represent empty or null string
                
            // Remove excess whitespace
            var trimmed = Regex.Replace(comment, @"\s+", " ").Trim();
            
            // Truncate very long comments (over 200 chars)
            if (trimmed.Length > 200)
            {
                trimmed = trimmed.Substring(0, 197) + "...";
            }
            
            return GetOrCreateStringId(trimmed);
        }

        /// <summary>
        /// Compresses method signatures to a shorter format
        /// </summary>
        private int CompressMethodSignature(MethodModel method)
        {
            var sb = new StringBuilder();
            
            // Use abbreviations for common access modifiers
            string accessMod = method.AccessModifier?.ToLower() switch
            {
                "public" => "pub",
                "private" => "prv",
                "protected" => "pro",
                "internal" => "int",
                "protected internal" => "pri",
                _ => method.AccessModifier
            };
            
            sb.Append(accessMod);
            
            // Add flags as single chars
            if (method.IsStatic) sb.Append(" s");
            if (method.IsAbstract) sb.Append(" a");
            if (method.IsVirtual) sb.Append(" v");
            if (method.IsOverride) sb.Append(" o");
            if (method.IsAsync) sb.Append(" as");
            
            // Add return type and name
            sb.Append(' ').Append(method.ReturnType).Append(' ').Append(method.Name);
            
            // Add parameters in compressed format
            sb.Append('(');
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var param = method.Parameters[i];
                if (i > 0) sb.Append(',');
                
                if (param.IsOut) sb.Append("out ");
                else if (param.IsByRef) sb.Append("ref ");
                else if (param.IsParams) sb.Append("params ");
                
                sb.Append(param.Type).Append(' ').Append(param.Name);
                
                if (param.IsOptional)
                {
                    sb.Append('=').Append(param.DefaultValue ?? "null");
                }
            }
            sb.Append(')');
            
            return GetOrCreateStringId(sb.ToString());
        }

        /// <summary>
        /// Returns a unique ID for a string, creating a new one if the string hasn't been seen before
        /// </summary>
        private int GetOrCreateStringId(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
                
            if (_stringIdMap.TryGetValue(str, out int id))
                return id;
                
            id = _nextStringId++;
            _stringIdMap[str] = id;
            return id;
        }

        #endregion
    }
} 