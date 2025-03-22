using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SeeSharpIndexer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SeeSharpIndexer.Services
{
    /// <summary>
    /// Service for parsing C# code files
    /// </summary>
    public class CodeParser
    {
        private readonly IndexerSettings _settings;

        public CodeParser(IndexerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Parse a C# file and extract its structure
        /// </summary>
        public async Task<FileItem> ParseFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            string fileContent = await File.ReadAllTextAsync(filePath);
            
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
            CompilationUnitSyntax? root = await syntaxTree.GetRootAsync() as CompilationUnitSyntax;

            if (root == null)
                throw new InvalidOperationException("Failed to parse file");

            var fileItem = new FileItem
            {
                Name = Path.GetFileName(filePath),
                FilePath = filePath,
                Location = filePath
            };

            // Process all classes in the file
            foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var classItem = ParseClass(classDeclaration);
                fileItem.Classes.Add(classItem);
            }

            // Process all interfaces in the file
            foreach (var interfaceDeclaration in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            {
                var interfaceItem = ParseInterface(interfaceDeclaration);
                fileItem.Interfaces.Add(interfaceItem);
            }

            // Process all enums in the file
            foreach (var enumDeclaration in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
            {
                var enumItem = ParseEnum(enumDeclaration);
                fileItem.Enums.Add(enumItem);
            }

            return fileItem;
        }

        private ClassItem ParseClass(ClassDeclarationSyntax classDeclaration)
        {
            var classItem = new ClassItem
            {
                Name = classDeclaration.Identifier.Text,
                Location = $"{classDeclaration.SyntaxTree.FilePath}:{classDeclaration.Span.Start}",
                Accessibility = GetAccessibility(classDeclaration.Modifiers),
                IsStatic = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsAbstract = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))
            };

            // Get base types
            if (classDeclaration.BaseList != null)
            {
                foreach (var baseType in classDeclaration.BaseList.Types)
                {
                    classItem.BaseTypes.Add(baseType.ToString());
                }
            }

            // Process methods
            foreach (var methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var accessibility = GetAccessibility(methodDeclaration.Modifiers);
                
                // Skip based on accessibility settings
                if (!ShouldIncludeMember(accessibility))
                    continue;

                var methodItem = ParseMethod(methodDeclaration);
                classItem.Methods.Add(methodItem);
            }

            // Process properties
            foreach (var propertyDeclaration in classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                var accessibility = GetAccessibility(propertyDeclaration.Modifiers);

                // Skip based on accessibility settings
                if (!ShouldIncludeMember(accessibility))
                    continue;

                var propertyItem = ParseProperty(propertyDeclaration);
                classItem.Properties.Add(propertyItem);
            }

            // Process fields if enabled in settings
            if (_settings.IncludeFields)
            {
                foreach (var fieldDeclaration in classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    var accessibility = GetAccessibility(fieldDeclaration.Modifiers);

                    // Skip based on accessibility settings
                    if (!ShouldIncludeMember(accessibility))
                        continue;

                    // Process each variable in the field declaration
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        var fieldItem = new FieldItem
                        {
                            Name = variable.Identifier.Text,
                            FieldType = fieldDeclaration.Declaration.Type.ToString(),
                            Accessibility = accessibility,
                            Location = $"{fieldDeclaration.SyntaxTree.FilePath}:{fieldDeclaration.Span.Start}",
                            IsStatic = fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                            IsReadOnly = fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword))
                        };

                        classItem.Fields.Add(fieldItem);
                    }
                }
            }

            return classItem;
        }

        private InterfaceItem ParseInterface(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceItem = new InterfaceItem
            {
                Name = interfaceDeclaration.Identifier.Text,
                Location = $"{interfaceDeclaration.SyntaxTree.FilePath}:{interfaceDeclaration.Span.Start}",
                Accessibility = GetAccessibility(interfaceDeclaration.Modifiers)
            };

            // Get base interfaces
            if (interfaceDeclaration.BaseList != null)
            {
                foreach (var baseType in interfaceDeclaration.BaseList.Types)
                {
                    interfaceItem.BaseInterfaces.Add(baseType.ToString());
                }
            }

            // Process methods
            foreach (var methodDeclaration in interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var methodItem = ParseMethod(methodDeclaration);
                interfaceItem.Methods.Add(methodItem);
            }

            // Process properties
            foreach (var propertyDeclaration in interfaceDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                var propertyItem = ParseProperty(propertyDeclaration);
                interfaceItem.Properties.Add(propertyItem);
            }

            return interfaceItem;
        }

        private EnumItem ParseEnum(EnumDeclarationSyntax enumDeclaration)
        {
            var enumItem = new EnumItem
            {
                Name = enumDeclaration.Identifier.Text,
                Location = $"{enumDeclaration.SyntaxTree.FilePath}:{enumDeclaration.Span.Start}",
                Accessibility = GetAccessibility(enumDeclaration.Modifiers)
            };

            // Process enum members
            foreach (var enumMember in enumDeclaration.Members)
            {
                enumItem.Values.Add(enumMember.Identifier.Text);
            }

            return enumItem;
        }

        private MethodItem ParseMethod(MethodDeclarationSyntax methodDeclaration)
        {
            var methodItem = new MethodItem
            {
                Name = methodDeclaration.Identifier.Text,
                ReturnType = methodDeclaration.ReturnType.ToString(),
                Location = $"{methodDeclaration.SyntaxTree.FilePath}:{methodDeclaration.Span.Start}",
                Accessibility = GetAccessibility(methodDeclaration.Modifiers),
                IsStatic = methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsAbstract = methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                IsVirtual = methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword))
            };

            // Process parameters
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                var parameterItem = new ParameterItem
                {
                    Name = parameter.Identifier.Text,
                    Type = parameter.Type.ToString()
                };

                // Add default value if present
                if (parameter.Default != null)
                {
                    parameterItem.DefaultValue = parameter.Default.Value.ToString();
                }

                methodItem.Parameters.Add(parameterItem);
            }

            return methodItem;
        }

        private PropertyItem ParseProperty(PropertyDeclarationSyntax propertyDeclaration)
        {
            var propertyItem = new PropertyItem
            {
                Name = propertyDeclaration.Identifier.Text,
                PropertyType = propertyDeclaration.Type.ToString(),
                Location = $"{propertyDeclaration.SyntaxTree.FilePath}:{propertyDeclaration.Span.Start}",
                Accessibility = GetAccessibility(propertyDeclaration.Modifiers),
                IsStatic = propertyDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            };

            // Determine if it has getter and setter
            var accessors = propertyDeclaration.AccessorList?.Accessors.ToList();
            
            if (accessors != null)
            {
                propertyItem.HasGetter = accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                propertyItem.HasSetter = accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            }
            else if (propertyDeclaration.ExpressionBody != null)
            {
                // Expression-bodied property (has getter only)
                propertyItem.HasGetter = true;
                propertyItem.HasSetter = false;
            }

            return propertyItem;
        }

        private string GetAccessibility(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return "public";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                return "private";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
            {
                if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                    return "protected internal";
                return "protected";
            }
            if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
                return "internal";
            
            return "internal"; // Default accessibility
        }

        private bool ShouldIncludeMember(string accessibility)
        {
            return accessibility switch
            {
                "public" => true,
                "private" => _settings.IncludePrivateMembers,
                "protected" => _settings.IncludeProtectedMembers,
                "internal" => _settings.IncludeInternalMembers,
                "protected internal" => _settings.IncludeProtectedMembers || _settings.IncludeInternalMembers,
                _ => true
            };
        }
    }
} 