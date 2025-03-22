# SeeSharpIndexer TODO Checklist

## 1. Core Architecture & Base Components
- [x] Define the `CodebaseIndexer` core class for managing the indexing process
- [x] Create `LanguageParser` abstract base class/interface for language-specific parsers
- [x] Implement language parser for C# (prioritize one language first)
- [x] Design the `IndexSerializer` class for CBOR output
- [x] Create `TokenOptimizer` class for compressing extracted metadata

## 2. Data Models
- [x] Define `CodebaseStructure` model to represent the entire codebase
- [x] Create `FileNode` model to represent individual source files
- [x] Design `ClassModel` to represent classes and their relationships
- [x] Implement `MethodModel` for function/method signatures
- [x] Create `PropertyModel` for class properties
- [x] Design `RelationshipModel` for inheritance and dependency mapping
- [x] Define `IndexSchema` for CBOR serialization format

## 3. Core Indexing Pipeline
- [x] Implement directory scanner for recursively analyzing codebases
- [x] Create file type detection system to identify language of each file
- [x] Build basic syntax parsing for C# files
- [x] Implement metadata extraction for classes, methods, properties
- [x] Create simple inheritance and relationship analyzer
- [x] Implement comment/documentation extractor
- [x] Build token optimization module to minimize output size
- [x] Create CBOR serialization module

## 4. UI Implementation
- [x] Create dark mode theme resources and styling
- [x] Design and implement the main window layout
  - [x] Folder selection controls
  - [x] Progress indicators
  - [x] Output path selection
- [x] Implement file browser dialog for codebase selection
- [x] Create indexing progress visualization (progress bar, status messages)
- [x] Add export functionality

## 5. External Dependencies
- [x] Add CBOR library (PeterO.Cbor or similar)
- [x] Integrate Roslyn for C# parsing

## 6. Token Optimization Techniques
- [x] Implement basic comment summarization
- [x] Create method signature compressor
- [x] Build simple reference deduplication system 