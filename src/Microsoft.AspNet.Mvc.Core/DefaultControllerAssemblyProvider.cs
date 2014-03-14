using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Net.Runtime;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerAssemblyProvider : IControllerAssemblyProvider
    {
        // List of Mvc assemblies that we'll use as roots for controller discovery.
        private static readonly HashSet<string> _mvcAssemblyList = new HashSet<string>(StringComparer.Ordinal)
        {
            "Microsoft.AspNet.Mvc",
            "Microsoft.AspNet.Mvc.Core",
            "Microsoft.AspNet.Mvc.ModelBinding",
            "Microsoft.AspNet.Mvc.Razor",
            "Microsoft.AspNet.Mvc.Razor.Host",
            "Microsoft.AspNet.Mvc.Rendering",
        };
        private readonly ILibraryManager _libraryManager;
        private readonly Func<ILibraryInformation, Assembly> _assemblyLoader;

        public DefaultControllerAssemblyProvider(ILibraryManager libraryManager) : 
            this (libraryManager, Load)
        {
            
        }

        internal DefaultControllerAssemblyProvider(ILibraryManager libraryManager, 
                                                   Func<ILibraryInformation, Assembly> assemblyLoader)
        {
            _libraryManager = libraryManager;
            _assemblyLoader = assemblyLoader;
        }

        public IEnumerable<Assembly> CandidateAssemblies
        {
            get
            {
                // Find all assemblies that reference any of the well-known Mvc assemblies
                // while ignoring the Mvc ones.
                return _mvcAssemblyList.SelectMany(_libraryManager.GetReferencingLibraries)
                                       .Distinct()
                                       .Where(IsCandidateLibrary)
                                       .Select(_assemblyLoader);
            }
        }

        private static Assembly Load(ILibraryInformation library)
        {
            return Assembly.Load(new AssemblyName(library.Name));
        }

        private static bool IsCandidateLibrary(ILibraryInformation library)
        {
            return !_mvcAssemblyList.Contains(library.Name);
        }
    }
}
