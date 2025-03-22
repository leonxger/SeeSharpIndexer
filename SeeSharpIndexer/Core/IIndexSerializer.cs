using SeeSharpIndexer.Models;
using System.Threading.Tasks;

namespace SeeSharpIndexer.Core
{
    public interface IIndexSerializer
    {
        Task SerializeAsync(CodebaseStructure codebase, string outputPath);
        Task<CodebaseStructure> DeserializeAsync(string filePath);
    }
} 