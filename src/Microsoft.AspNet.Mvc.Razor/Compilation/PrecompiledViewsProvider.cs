using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    public static class PrecompiledViewsProvider
    {
        public static void PopulateCache([NotNull] ICompilerCache compilerCache,
                                         [NotNull] IAssemblyProvider assemblyProvider)
        {
            foreach (var viewCollection in GetFileInfos(assemblyProvider))
            {
                foreach (var fileInfo in viewCollection.FileInfos)
                {
                    var containingAssembly = viewCollection.GetType().GetTypeInfo().Assembly;
                    var viewType = containingAssembly.GetType(fileInfo.FullTypeName);

                    // There shouldn't be any duplicates and if there are any the first will win.
                    // If the result doesn't match the one on disk its going to recompile anyways.
                    compilerCache.Add(fileInfo, viewType);
                }
            }
        }

        internal static IEnumerable<RazorFileInfoCollection> GetFileInfos(IAssemblyProvider assemblyProvider)
        {
            return assemblyProvider
                    .CandidateAssemblies
                    .SelectMany(a => a.ExportedTypes)
                    .Where(Match)
                    .Select(c => (RazorFileInfoCollection)Activator.CreateInstance(c));
        }

        private static bool Match(Type t)
        {
            var inAssemblyType = typeof(RazorFileInfoCollection);
            if (inAssemblyType.IsAssignableFrom(t))
            {
                var hasParameterlessConstructor = t.GetConstructor(Type.EmptyTypes) != null;

                return hasParameterlessConstructor
                    && !t.GetTypeInfo().IsAbstract
                    && !t.GetTypeInfo().ContainsGenericParameters;
            }

            return false;
        }
    }
}