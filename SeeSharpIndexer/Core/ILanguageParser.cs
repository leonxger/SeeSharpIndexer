using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SeeSharpIndexer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpIndexer.Core
{
    public interface ILanguageParser
    {
        Task<List<ClassModel>> ParseFileAsync(string filePath, string sourceCode);
        bool CanParseFileExtension(string extension);
    }
}
