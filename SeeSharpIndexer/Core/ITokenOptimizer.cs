using SeeSharpIndexer.Models;
using System.Collections.Generic;

namespace SeeSharpIndexer.Core
{
    public interface ITokenOptimizer
    {
        void OptimizeClassModels(List<ClassModel> classModels);
    }
} 