// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHost : RazorEngineHost, IMvcRazorHost
    {
        private const string ViewNamespace = "ASP";

        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "Microsoft.AspNet.Mvc",
            "Microsoft.AspNet.Mvc.Razor",
            "Microsoft.AspNet.Mvc.Rendering"
        };

        // CodeGenerationContext.DefaultBaseClass is set to MyBaseType<dynamic>. 
        // This field holds the type name without the generic decoration (MyBaseType)
        private readonly string _baseType;
        private readonly string _viewStartBaseType;

        public MvcRazorHost(Type baseType,
                            Type viewStartBaseType)
            : this(baseType.FullName,
                  viewStartBaseType.FullName)
        {
        }

        public MvcRazorHost(string baseType,
                            string viewStartBaseType)
            : base(new CSharpRazorCodeLanguage())
        {
            _baseType = baseType;
            _viewStartBaseType = viewStartBaseType;

            GeneratedClassContext = new GeneratedClassContext(
                executeMethodName: "ExecuteAsync",
                writeMethodName: "Write",
                writeLiteralMethodName: "WriteLiteral",
                writeToMethodName: "WriteTo",
                writeLiteralToMethodName: "WriteLiteralTo",
                templateTypeName: "HelperResult",
                defineSectionMethodName: "DefineSection")
            {
                ResolveUrlMethodName = "Href"
            };

            foreach (var ns in _defaultNamespaces)
            {
                NamespaceImports.Add(ns);
            }
        }

        public IViewStartProvider ViewStartProvider { get; set; }

        public GeneratorResults GenerateCode(string rootRelativePath,
                                             Stream inputStream)
        {
            if (ViewStartProvider.IsViewStart(rootRelativePath))
            {
                DefaultBaseClass = _viewStartBaseType;
            }
            else
            {
                DefaultBaseClass = _baseType + "<dynamic>";
            }

            var className = ParserHelpers.SanitizeClassName(rootRelativePath);
            using (var reader = new StreamReader(inputStream))
            {
                var engine = new RazorTemplateEngine(this);
                return engine.GenerateCode(reader, className, ViewNamespace, rootRelativePath);
            }
        }

        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            return new MvcRazorCodeParser(_baseType);
        }

        public override CodeBuilder DecorateCodeBuilder(CodeBuilder incomingBuilder, CodeGeneratorContext context)
        {
            if (ViewStartProvider.IsViewStart(context.SourceFile))
            {
                return incomingBuilder;
            }
            else
            {
                UpdateCodeGeneratorContext(context.SourceFile, context);
                return new MvcCSharpCodeBuilder(context);
            }
        }

        private void UpdateCodeGeneratorContext(string viewFile,
                                                CodeGeneratorContext context)
        {
            var engine = new RazorTemplateEngine(this);
            var currentChunks = context.CodeTreeBuilder.CodeTree.Chunks;
            var existingInjects = new HashSet<string>(currentChunks.OfType<InjectChunk>()
                                                                   .Select(c => c.MemberName),
                                                      StringComparer.OrdinalIgnoreCase);

            var existingUsings = new HashSet<string>(currentChunks.OfType<UsingChunk>()
                                                                  .Select(c => c.Namespace),
                                                     StringComparer.Ordinal);


            // When adding chunks, work outside in. This makes it easier for interesting chunks injected in nearer 
            // views to supersede chunksin ViewStarts that are further away.
            foreach (var viewStart in ViewStartProvider.GetViewStartLocations(viewFile).Reverse())
            {
                var parsedTree = ParseViewFile(engine, viewStart);

                var injecttChunksToAdd = parsedTree.Chunks.OfType<InjectChunk>()
                                                         .Where(c => !existingInjects.Contains(c.MemberName));
                foreach (var injectChunk in injecttChunksToAdd)
                {
                    currentChunks.Add(injectChunk);
                    existingInjects.Add(injectChunk.MemberName);
                }

                var usingsToAdd = parsedTree.Chunks.OfType<UsingChunk>()
                                                   .Where(c => !existingUsings.Contains(c.Namespace));

                foreach (var usingChunk in usingsToAdd)
                {
                    currentChunks.Add(usingChunk);
                    existingUsings.Add(usingChunk.Namespace);
                }
            }
        }

        private static CodeTree ParseViewFile(RazorTemplateEngine engine, string viewStart)
        {
            using (var stream = File.OpenRead(viewStart))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var parseResults = engine.ParseTemplate(streamReader);
                    var className = ParserHelpers.SanitizeClassName(viewStart);
                    var codeGenerator = engine.Host.CodeLanguage.CreateCodeGenerator(className,
                                                                                     ViewNamespace,
                                                                                     viewStart,
                                                                                     engine.Host);
                    codeGenerator.Visit(parseResults);
                    return codeGenerator.Context.CodeTreeBuilder.CodeTree;
                }
            }
        }
    }
}
