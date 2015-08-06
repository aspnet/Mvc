// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNet.Mvc.Razor.Precompilation;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Internal
{
    public static class RazorFileInfoCollectionGenerator
    {
        public static string GenerateCollection([NotNull] RazorFileInfoCollection fileInfoCollection)
        {
            var builder = new StringBuilder();

            builder.Append(
$@"namespace __ASP_ASSEMBLY
{{
    [{typeof(CompilerGeneratedAttribute).FullName}]
    public class __PreGeneratedViewCollection : {typeof(RazorFileInfoCollection).FullName}
    {{
        public __PreGeneratedViewCollection()
        {{
            {nameof(RazorFileInfoCollection.AssemblyResourceName)} = @""{fileInfoCollection.AssemblyResourceName}"";
            {nameof(RazorFileInfoCollection.SymbolsResourceName)} = @""{fileInfoCollection.SymbolsResourceName}"";
            FileInfos = new System.Collections.Generic.List<{typeof(RazorFileInfo).FullName}>
            {{");

            foreach (var fileInfo in fileInfoCollection.FileInfos)
            {
                builder.Append(
$@"             
                new {typeof(RazorFileInfo).FullName}
                {{
                    {nameof(RazorFileInfo.FullTypeName)} = @""{fileInfo.FullTypeName}"",
                    {nameof(RazorFileInfo.RelativePath)} = @""{fileInfo.RelativePath}""
                }},");
            }

            builder.Append(
$@"
            }};
        }}

        private static {typeof(System.Reflection.Assembly).FullName} _loadedAssembly;

        public override {typeof(System.Reflection.Assembly).FullName} LoadAssembly(
            {typeof(IAssemblyLoadContext).FullName} loadContext)
        {{
             if (_loadedAssembly == null)
             {{
                _loadedAssembly = base.LoadAssembly(loadContext);
             }}
             return _loadedAssembly;   
        }}
    }}
}}");
            return builder.ToString();
        }
    }
}