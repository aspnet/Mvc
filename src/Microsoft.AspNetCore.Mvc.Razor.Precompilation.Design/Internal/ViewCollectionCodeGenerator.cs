// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Core.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class ViewCollectionCodeGenerator
    {
        public ViewCollectionCodeGenerator(
            CSharpCompiler compiler,
            CSharpCompilation compilation)
        {
            Compiler = compiler;
            Compilation = compilation;
        }

        public CSharpCompiler Compiler { get; }

        public CSharpCompilation Compilation { get; private set; }

        public void AddViewFactory(List<ViewCompilationInfo> result)
        {
            var precompiledViewsArray = new StringBuilder();
            foreach (var item in result)
            {
                precompiledViewsArray.AppendLine(
                    $"new {typeof(PrecompiledViewInfo).FullName}(@\"{item.RelativeFileInfo.RelativePath}\", typeof({item.TypeName})),");
            }

            var factoryContent =
            $@"
namespace {AssemblyPart.ViewCollectionNamespace}
{{
  public class {AssemblyPart.ViewCollectionTypeName} : {typeof(PrecompiledViews).FullName}
  {{
    public {AssemblyPart.ViewCollectionTypeName}() : base(new[]
    {{
        {precompiledViewsArray}
    }})
    {{
    }}
  }}
}}";
            var syntaxTree = Compiler.CreateSyntaxTree(SourceText.From(factoryContent));
            Compilation = Compilation.AddSyntaxTrees(syntaxTree);
        }

        public void AddAssemblyMetadata(
            AssemblyName applicationAssemblyName,
            StrongNameOptions strongNameOptions)
        {
            if (!string.IsNullOrEmpty(strongNameOptions.KeyFile))
            {
                var updatedOptions = Compilation.Options.WithStrongNameProvider(new DesktopStrongNameProvider());

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)  || strongNameOptions.PublicSign)
                {
                    updatedOptions = updatedOptions.WithCryptoPublicKey(
                        SnkUtils.ExtractPublicKey(File.ReadAllBytes(strongNameOptions.KeyFile)));
                }
                else
                {
                    updatedOptions = updatedOptions.WithCryptoKeyFile(strongNameOptions.KeyFile)
                        .WithDelaySign(strongNameOptions.DelaySign);
                }

                Compilation = Compilation.WithOptions(updatedOptions);
            }

            var assemblyVersionContent = $"[assembly:{typeof(AssemblyVersionAttribute).FullName}(\"{applicationAssemblyName.Version}\")]";
            var syntaxTree = Compiler.CreateSyntaxTree(SourceText.From(assemblyVersionContent));
            Compilation = Compilation.AddSyntaxTrees(syntaxTree);
        }
    }
}
