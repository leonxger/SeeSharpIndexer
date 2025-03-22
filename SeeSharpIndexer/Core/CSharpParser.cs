using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SeeSharpIndexer.Models;

namespace SeeSharpIndexer.Core
{
    /// <summary>
    /// C# language parser that uses Roslyn to analyze code
    /// </summary>
    public class CSharpParser : ILanguageParser
    {
        /// <summary>
        /// Supported file extensions
        /// </summary>
        private readonly string[] _supportedExtensions = { ".cs" };

        /// <summary>
        /// Determines if this parser can handle the given file extension
        /// </summary>
        /// <param name="extension">The file extension to check</param>
        /// <returns>True if the parser can handle this file extension, false otherwise</returns>
        public bool CanParseFileExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            // Make sure extension starts with a dot
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return _supportedExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Determines if this parser can handle the given file type
        /// </summary>
        /// <param name="filePath">Path to the file to be checked</param>
        /// <returns>True if the parser can handle this file type, false otherwise</returns>
        public bool CanHandleFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return CanParseFileExtension(extension);
        }

        /// <summary>
        /// Parses a C# source code file and extracts its structure
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <param name="sourceCode">Source code content</param>
        /// <returns>List of parsed classes from the file</returns>
        public async Task<List<ClassModel>> ParseFileAsync(string filePath, string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
                throw new ArgumentException("Source code cannot be null or empty", nameof(sourceCode));

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = await syntaxTree.GetRootAsync() as CompilationUnitSyntax;

            if (root == null)
                throw new InvalidOperationException("Failed to parse the C# file");

            // Extract classes
            return ExtractClasses(root);
        }

        /// <summary>
        /// Parses a C# source code file and extracts its structure
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns>Parsed file structure as a FileNode</returns>
        public async Task<object> ParseFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            string sourceCode = await File.ReadAllTextAsync(filePath);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = await syntaxTree.GetRootAsync() as CompilationUnitSyntax;

            if (root == null)
                throw new InvalidOperationException("Failed to parse the C# file");

            // Create file node
            var fileInfo = new FileInfo(filePath);
            var fileNode = new FileNode
            {
                AbsolutePath = Path.GetFullPath(filePath),
                RelativePath = Path.GetFileName(filePath), // This should be relative to the root in real implementation
                Language = "C#",
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime
            };

            // Extract namespaces
            fileNode.Namespaces = ExtractNamespaces(root);

            // Extract using statements
            fileNode.Imports = ExtractImports(root);

            // Extract classes
            fileNode.Classes = ExtractClasses(root);

            return fileNode;
        }

        /// <summary>
        /// Extracts namespaces from the compilation unit
        /// </summary>
        private List<string> ExtractNamespaces(CompilationUnitSyntax root)
        {
            var namespaces = new List<string>();

            // Add all namespace declarations
            foreach (var namespaceDecl in root.DescendantNodes().OfType<NamespaceDeclarationSyntax>())
            {
                namespaces.Add(namespaceDecl.Name.ToString());
            }

            // Add all file-scoped namespace declarations (C# 10+)
            foreach (var fileScopedNamespace in root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>())
            {
                namespaces.Add(fileScopedNamespace.Name.ToString());
            }

            return namespaces;
        }

        /// <summary>
        /// Extracts using statements from the compilation unit
        /// </summary>
        private List<string> ExtractImports(CompilationUnitSyntax root)
        {
            return root.Usings
                .Select(u => u.Name.ToString())
                .ToList();
        }

        /// <summary>
        /// Extracts classes from the compilation unit
        /// </summary>
        private List<ClassModel> ExtractClasses(CompilationUnitSyntax root)
        {
            var classes = new List<ClassModel>();

            // Find all class declarations
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDecl in classDeclarations)
            {
                var classModel = new ClassModel
                {
                    Name = classDecl.Identifier.Text,
                    ClassType = "class",
                    IsStatic = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    IsAbstract = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    IsSealed = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)),
                    IsPartial = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                    AccessModifier = GetAccessModifier(classDecl.Modifiers),
                };

                // Get class's namespace
                var namespaceDecl = classDecl.Parent as NamespaceDeclarationSyntax;
                if (namespaceDecl != null)
                {
                    classModel.Namespace = namespaceDecl.Name.ToString();
                    classModel.FullyQualifiedName = $"{namespaceDecl.Name}.{classModel.Name}";
                }
                else
                {
                    var fileScopedNamespace = classDecl.Parent as FileScopedNamespaceDeclarationSyntax;
                    if (fileScopedNamespace != null)
                    {
                        classModel.Namespace = fileScopedNamespace.Name.ToString();
                        classModel.FullyQualifiedName = $"{fileScopedNamespace.Name}.{classModel.Name}";
                    }
                    else
                    {
                        classModel.Namespace = "";
                        classModel.FullyQualifiedName = classModel.Name;
                    }
                }

                // Check if nested class
                classModel.IsNested = classDecl.Parent is ClassDeclarationSyntax;
                if (classModel.IsNested)
                {
                    var parentClass = classDecl.Parent as ClassDeclarationSyntax;
                    classModel.ParentClassName = parentClass.Identifier.Text;
                }

                // Extract methods
                classModel.Methods = ExtractMethods(classDecl);

                // Extract properties
                classModel.Properties = ExtractProperties(classDecl);

                // Extract documentation
                classModel.Documentation = ExtractDocumentation(classDecl);

                classes.Add(classModel);
            }

            // Extract interfaces, structs, and enums (simplified for now)
            ExtractInterfaces(root, classes);
            ExtractStructs(root, classes);
            ExtractEnums(root, classes);

            return classes;
        }

        /// <summary>
        /// Extracts the access modifier from a list of modifiers
        /// </summary>
        private string GetAccessModifier(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return "public";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                return "private";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
                return "protected";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                return "internal";
            
            return "internal"; // Default for C#
        }

        /// <summary>
        /// Extracts method models from a class declaration
        /// </summary>
        private List<MethodModel> ExtractMethods(ClassDeclarationSyntax classDecl)
        {
            var methods = new List<MethodModel>();

            foreach (var methodDecl in classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var methodModel = new MethodModel
                {
                    Name = methodDecl.Identifier.Text,
                    ReturnType = methodDecl.ReturnType.ToString(),
                    AccessModifier = GetAccessModifier(methodDecl.Modifiers),
                    IsStatic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    IsVirtual = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                    IsOverride = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                    IsAbstract = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    Documentation = ExtractDocumentation(methodDecl)
                };

                // Extract parameters
                foreach (var param in methodDecl.ParameterList.Parameters)
                {
                    methodModel.Parameters.Add(new Models.ParameterModel
                    {
                        Name = param.Identifier.Text,
                        Type = param.Type?.ToString() ?? "unknown",
                        HasDefaultValue = param.Default != null
                    });
                }

                methods.Add(methodModel);
            }

            return methods;
        }

        /// <summary>
        /// Extracts property models from a class declaration
        /// </summary>
        private List<PropertyModel> ExtractProperties(ClassDeclarationSyntax classDecl)
        {
            var properties = new List<PropertyModel>();

            foreach (var propDecl in classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                var propModel = new PropertyModel
                {
                    Name = propDecl.Identifier.Text,
                    Type = propDecl.Type.ToString(),
                    AccessModifier = GetAccessModifier(propDecl.Modifiers),
                    IsStatic = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    IsVirtual = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                    IsOverride = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                    IsAbstract = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    HasGetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,
                    HasSetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,
                    Documentation = ExtractDocumentation(propDecl)
                };

                properties.Add(propModel);
            }

            return properties;
        }

        /// <summary>
        /// Extracts documentation comments from a syntax node
        /// </summary>
        private string ExtractDocumentation(SyntaxNode node)
        {
            // Get the leading trivia of the node
            var leadingTrivia = node.GetLeadingTrivia();
            var documentationTrivia = leadingTrivia
                .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                            t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                .ToList();

            if (documentationTrivia.Any())
            {
                return string.Join(Environment.NewLine, documentationTrivia.Select(t => t.ToString()));
            }

            return string.Empty;
        }

        /// <summary>
        /// Extracts interface declarations and adds them to the classes list
        /// </summary>
        private void ExtractInterfaces(CompilationUnitSyntax root, List<ClassModel> classes)
        {
            var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            foreach (var interfaceDecl in interfaceDeclarations)
            {
                var interfaceModel = new ClassModel
                {
                    Name = interfaceDecl.Identifier.Text,
                    ClassType = "interface",
                    AccessModifier = GetAccessModifier(interfaceDecl.Modifiers),
                    IsPartial = interfaceDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                    Documentation = ExtractDocumentation(interfaceDecl)
                };

                // Set namespace and fully qualified name
                var namespaceDecl = interfaceDecl.Parent as NamespaceDeclarationSyntax;
                if (namespaceDecl != null)
                {
                    interfaceModel.Namespace = namespaceDecl.Name.ToString();
                    interfaceModel.FullyQualifiedName = $"{namespaceDecl.Name}.{interfaceModel.Name}";
                }
                else
                {
                    var fileScopedNamespace = interfaceDecl.Parent as FileScopedNamespaceDeclarationSyntax;
                    if (fileScopedNamespace != null)
                    {
                        interfaceModel.Namespace = fileScopedNamespace.Name.ToString();
                        interfaceModel.FullyQualifiedName = $"{fileScopedNamespace.Name}.{interfaceModel.Name}";
                    }
                    else
                    {
                        interfaceModel.Namespace = "";
                        interfaceModel.FullyQualifiedName = interfaceModel.Name;
                    }
                }

                // Check if nested
                interfaceModel.IsNested = interfaceDecl.Parent is ClassDeclarationSyntax ||
                                          interfaceDecl.Parent is StructDeclarationSyntax;
                if (interfaceModel.IsNested)
                {
                    if (interfaceDecl.Parent is ClassDeclarationSyntax classParent)
                    {
                        interfaceModel.ParentClassName = classParent.Identifier.Text;
                    }
                    else if (interfaceDecl.Parent is StructDeclarationSyntax structParent)
                    {
                        interfaceModel.ParentClassName = structParent.Identifier.Text;
                    }
                }

                classes.Add(interfaceModel);
            }
        }

        /// <summary>
        /// Extracts struct declarations and adds them to the classes list
        /// </summary>
        private void ExtractStructs(CompilationUnitSyntax root, List<ClassModel> classes)
        {
            var structDeclarations = root.DescendantNodes().OfType<StructDeclarationSyntax>();
            foreach (var structDecl in structDeclarations)
            {
                var structModel = new ClassModel
                {
                    Name = structDecl.Identifier.Text,
                    ClassType = "struct",
                    AccessModifier = GetAccessModifier(structDecl.Modifiers),
                    IsPartial = structDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                    Documentation = ExtractDocumentation(structDecl)
                };

                classes.Add(structModel);
            }
        }

        /// <summary>
        /// Extracts enum declarations and adds them to the classes list
        /// </summary>
        private void ExtractEnums(CompilationUnitSyntax root, List<ClassModel> classes)
        {
            var enumDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
            foreach (var enumDecl in enumDeclarations)
            {
                var enumModel = new ClassModel
                {
                    Name = enumDecl.Identifier.Text,
                    ClassType = "enum",
                    AccessModifier = GetAccessModifier(enumDecl.Modifiers),
                    Documentation = ExtractDocumentation(enumDecl)
                };

                classes.Add(enumModel);
            }
        }
    }

    /// <summary>
    /// Represents a method parameter
    /// </summary>
    public class ParameterModel
    {
        /// <summary>
        /// Gets or sets the name of the parameter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the parameter
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the parameter has a default value
        /// </summary>
        public bool HasDefaultValue { get; set; }
    }
} 