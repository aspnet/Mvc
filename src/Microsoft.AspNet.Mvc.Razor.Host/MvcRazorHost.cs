// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Mvc.Razor.Directives;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHost : RazorEngineHost, IMvcRazorHost
    {
        internal const string ViewNamespace = "ASP";
        internal const string DefaultModel = "dynamic";
        private const string BaseType = "Microsoft.AspNet.Mvc.Razor.RazorPage";
        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "Microsoft.AspNet.Mvc",
            "Microsoft.AspNet.Mvc.Razor",
            "Microsoft.AspNet.Mvc.Rendering"
        };

        private readonly MvcRazorHostOptions _hostOptions;
        private readonly string _appRoot;
        private readonly IFileSystem _fileSystem;
        // CodeGenerationContext.DefaultBaseClass is set to MyBaseType<dynamic>. 
        // This field holds the type name without the generic decoration (MyBaseType)
        private readonly string _baseType;

        public MvcRazorHost(
                IApplicationEnvironment appEnvironment)
            : this(appEnvironment.ApplicationBasePath,
                   new PhysicalFileSystem(appEnvironment.ApplicationBasePath))
        {
        }

        protected internal MvcRazorHost(string appRoot,
                                        IFileSystem fileSystem)
            : base(new CSharpRazorCodeLanguage())
        {
            _appRoot = appRoot;
            _fileSystem = fileSystem;
            _baseType = BaseType;

            // TODO: this needs to flow from the application rather than being initialized here.
            // Tracked by #774
            _hostOptions = new MvcRazorHostOptions();
            DefaultBaseClass = BaseType + '<' + DefaultModel + '>';
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

        public GeneratorResults GenerateCode(string rootRelativePath, Stream inputStream)
        {
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
            UpdateCodeBuilder(context);
            return new MvcCSharpCodeBuilder(context, _hostOptions);
        }

        private void UpdateCodeBuilder(CodeGeneratorContext context)
        {
            var engine = new RazorTemplateEngine(this);
            var chunkUtility = new ChunkInheritanceUtility();
            var inheritedChunks = chunkUtility.GetInheritedChunks(engine, _fileSystem, _appRoot, context.SourceFile);
            chunkUtility.MergeInheritedChunks(context.CodeTreeBuilder.CodeTree, inheritedChunks);
        }

        private CodeTree ParseViewFile(RazorTemplateEngine engine, string viewStart)
        {
            IFileInfo fileInfo;
            _fileSystem.TryGetFileInfo(viewStart, out fileInfo);

            using (var stream = fileInfo.CreateReadStream())
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
