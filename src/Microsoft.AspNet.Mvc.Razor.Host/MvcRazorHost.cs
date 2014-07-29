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
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHost : RazorEngineHost, IMvcRazorHost
    {
        private readonly MvcRazorHostOptions _hostOptions;

        public MvcRazorHost(IServiceProvider serviceProvider)
            : this(GetHostOptions(serviceProvider))
        {
        }

        protected MvcRazorHost(MvcRazorHostOptions hostOptions)
            : base(new CSharpRazorCodeLanguage())
        {
            _hostOptions = hostOptions;
            DefaultBaseClass = _hostOptions.DefaultBaseClass + '<' + _hostOptions.DefaultModel + '>';
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

            foreach (var ns in hostOptions.DefaultImportedNamespaces)
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
                return engine.GenerateCode(reader, className, _hostOptions.DefaultNamespace, rootRelativePath);
            }
        }

        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            return new MvcRazorCodeParser(_hostOptions.DefaultBaseClass);
        }

        public override CodeBuilder DecorateCodeBuilder(CodeBuilder incomingBuilder, CodeGeneratorContext context)
        {
            UpdateCodeBuilder(context);
            return new MvcCSharpCodeBuilder(context, _hostOptions);
        }

        private void UpdateCodeBuilder(CodeGeneratorContext context)
        {
            var currentChunks = context.CodeTreeBuilder.CodeTree.Chunks;
            var existingInjects = new HashSet<string>(currentChunks.OfType<InjectChunk>()
                                                                   .Select(c => c.MemberName),
                                                      StringComparer.Ordinal);

            var modelChunk = currentChunks.OfType<ModelChunk>()
                                          .LastOrDefault();
            var model = _hostOptions.DefaultModel;
            if (modelChunk != null)
            {
                model = modelChunk.ModelType;
            }
            model = '<' + model + '>';

            // Locate properties by name that haven't already been injected in to the View.
            var propertiesToAdd = _hostOptions.DefaultInjectedProperties
                                              .Where(c => !existingInjects.Contains(c.MemberName));
            foreach (var property in propertiesToAdd)
            {
                var typeName = property.TypeName.Replace("<TModel>", model);
                currentChunks.Add(new InjectChunk(typeName, property.MemberName));
            }
        }

        private static MvcRazorHostOptions GetHostOptions(IServiceProvider serviceProvider)
        {
            IOptionsAccessor<MvcRazorHostOptions> optionsAccessor;
            try
            {
                // Try reading the IOptionsAccessor from the DI container in a safe manner. This is
                // neccessary to support design time scenarios where the application does not compile
                // and consequently the startup cannot be executed to provide the actual configured host options.
                optionsAccessor = serviceProvider.GetService<IOptionsAccessor<MvcRazorHostOptions>>();
            }
            catch
            {
                optionsAccessor = null;
            }

            return optionsAccessor == null ? new MvcRazorHostOptions() : optionsAccessor.Options;
        }
    }
}
