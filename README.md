# SeeSharp Indexer

☕ **Enjoying SeeSharp Indexer?** [Buy me a coffee!](https://buymeacoffee.com/leonxger) Support the development of this project. Your contribution helps!

<p align="center">
  <img src="https://github.com/leonxger/SeeSharpIndexer/raw/main/Assets/logo.png" alt="SeeSharp Indexer Logo" width="200">
</p>

## About

SeeSharp Indexer is a specialized tool designed to efficiently index C# codebases for AI processing and analysis. It creates optimized, token-efficient JSON representations of your code structure, making it easier for AI models to understand and work with large C# projects.

This tool uses Roslyn (the .NET compiler platform) to parse C# files and extract structural information about your codebase, including classes, interfaces, methods, properties, and relationships between them.

## Features

- **Comprehensive Code Analysis**: Scans C# files to extract detailed structural information
- **Customizable Output**: Configure exactly what gets included in your index
- **Token Efficiency**: Generates compact JSON optimized for AI consumption
- **Modern UI**: Clean, intuitive Material Design interface
- **Bulk Processing**: Add individual files or entire directories recursively
- **Filtering Options**: Include/exclude private, internal, or protected members
- **Save & Load**: Save generated indexes for later use or sharing

## How It Works

SeeSharp Indexer uses the Roslyn compiler platform to analyze your C# code at the Abstract Syntax Tree (AST) level, extracting structural information without executing your code. The tool captures:

- **Files**: All included C# files with their paths
- **Classes**: Class name, accessibility, inheritance, static/abstract status
- **Interfaces**: Interface name, accessibility, inheritance
- **Methods**: Method signature, return type, parameters, accessibility
- **Properties**: Property name, type, accessors, accessibility
- **Fields**: Field name, type, accessibility, static/readonly status
- **Enums**: Enum name, values, accessibility

This information is organized into a hierarchical JSON structure that's optimized for size and AI token efficiency.

## Getting Started

### Installation

1. Download the latest release from the [Releases page](https://github.com/leonxger/SeeSharpIndexer/releases)
2. Extract the zip file to your preferred location
3. Run `SeeSharpIndexer.exe` to start the application

No installation required! The application runs as a standalone executable.

### Usage

1. **Add Files or Directories**:
   - Click the "Add Files" button to select individual C# files
   - Click the "Add Directory" button to recursively add all C# files in a directory

2. **Configure Index Settings**:
   - Click the settings icon to open the settings panel
   - Choose which members to include (private, internal, protected)
   - Configure output options

3. **Add Codebase Information**:
   - Provide a name and description for your codebase (optional)

4. **Generate the Index**:
   - Click the "Scan" button to analyze your code
   - The tool will process all selected files and create an index

5. **Save the Index**:
   - Click the "Save" button to save the generated index as a JSON file
   - Choose your save location and filename

## Index Format

The generated index follows this JSON structure:

```json
{
  "name": "Project Name",
  "description": "Project description",
  "createdAt": "2023-06-15T10:30:00Z",
  "totalFileCount": 42,
  "totalClassCount": 86,
  "totalInterfaceCount": 14,
  "totalEnumCount": 8,
  "files": [
    {
      "name": "Program.cs",
      "type": "File",
      "filePath": "C:/Projects/MyProject/Program.cs",
      "classes": [
        {
          "name": "Program",
          "type": "Class",
          "accessibility": "public",
          "isStatic": true,
          "methods": [
            {
              "name": "Main",
              "type": "Method",
              "returnType": "void",
              "accessibility": "public",
              "isStatic": true,
              "parameters": [
                {
                  "name": "args",
                  "type": "string[]"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

## System Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- At least 4GB RAM (8GB recommended for large codebases)
- 100MB free disk space

## Building from Source

1. Clone the repository:
   ```
   git clone https://github.com/leonxger/SeeSharpIndexer.git
   ```

2. Open the solution in Visual Studio 2022 or later

3. Build the solution (requires .NET 8.0 SDK)

## Use Cases

- **AI Training**: Generate efficient representations of C# codebases for AI model training
- **Codebase Analysis**: Get a structured overview of your project
- **Documentation Generation**: Use the index as a basis for automated documentation
- **AI-Assisted Development**: Help AI tools better understand your codebase structure

## Contact

- GitHub: [leonxger](https://github.com/leonxger)
- Report issues: [Issue Tracker](https://github.com/leonxger/SeeSharpIndexer/issues)

## Acknowledgments

- [Roslyn](https://github.com/dotnet/roslyn) - The .NET Compiler Platform
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - UI components
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON parsing and generation 