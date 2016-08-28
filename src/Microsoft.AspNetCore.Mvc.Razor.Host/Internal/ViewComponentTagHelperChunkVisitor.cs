using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Internal
{
    public class ViewComponentTagHelperChunkVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private GeneratedViewComponentTagHelperContext _context;
        private HashSet<TagHelperChunk> _writtenChunks;

        public ViewComponentTagHelperChunkVisitor(CSharpCodeWriter writer, CodeGeneratorContext context) :
            base(writer, context)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = new GeneratedViewComponentTagHelperContext();
            _writtenChunks = new HashSet<TagHelperChunk>();
        }

        public override void Accept(Chunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            // Has already parsed.
            if (_writtenChunks.Contains(chunk)) return;

            var parentChunk = chunk as ParentChunk;
            var tagHelperChunk = chunk as TagHelperChunk;

            // If not a tag helper chunk, we check the children.
            if (parentChunk != null && !(parentChunk is TagHelperChunk))
            {
                Accept(parentChunk.Children);
            }

            else if (tagHelperChunk != null) // Else, we check this tag helper chunk for view component properties.
            {
                var viewComponentDescriptors = tagHelperChunk.Descriptors.Where(
                    descriptor => ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor));
                foreach (var descriptor in viewComponentDescriptors)
                {
                    base.Accept(chunk);
                    return;
                }
            }
        }

        // Writes the view component tag helper class.
        protected override void Visit(TagHelperChunk chunk)
        {
            if (!_writtenChunks.Contains(chunk))
            {
                var viewComponentDescriptors = chunk.Descriptors.Where(
                    descriptor => ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor));

                foreach (var descriptor in viewComponentDescriptors)
                {
                    _writtenChunks.Add(chunk);
                    WriteClass(descriptor);
                    return;
                }
            }
        }

        private void WriteClass(TagHelperDescriptor descriptor)
        {
            // Add target element.
            BuildTargetElementString(descriptor);

            // Initialize declaration.
            var tagHelperTypeName = $"{_context.TagHelpersNamespace}.{nameof(TagHelper)}";

            var className = ViewComponentTagHelperDescriptorConventions.GetViewComponentTagHelperName(descriptor);

            using (Writer.BuildClassDeclaration("public", className, new[] { tagHelperTypeName }))
            {
                // Add view component helper for reasons.
                Writer.WriteVariableDeclaration(
                    $"private readonly {_context.IViewComponentHelperType}",
                    $"_viewComponentHelper", "");

                // Add constructor.
                BuildConstructorString(className);

                // Add attributes.
                BuildAttributeDeclarations(descriptor);

                // Add process method.
                BuildProcessMethodString(descriptor);
            }
        }

        private void BuildConstructorString(string className)
        {
            KeyValuePair<string, string> helperPair = new KeyValuePair<string, string>(
                _context.IViewComponentHelperType,
                "viewComponentHelper");

            using (Writer.BuildConstructor(
                "public",
                className,
                new List<KeyValuePair<string, string>>() { helperPair }))
            {
                Writer.WriteVariableDeclaration("", "_viewComponentHelper", "viewComponentHelper");
            }
        }

        private void BuildAttributeDeclarations(TagHelperDescriptor descriptor)
        {
            // Add view context.

            Writer.Write("[")
              .WriteMethodInvocation(typeof(HtmlAttributeNotBoundAttribute).FullName, false, new string[0])
              .WriteParameterSeparator()
              .Write(_context.ViewContextType)
              .Write("]")
              .WriteLine();

            Writer.WriteAutoPropertyDeclaration(
                "public",
                _context.ViewContextType,
                _context.ViewContextType);


            foreach (var attribute in descriptor.Attributes)
            {
                Writer.WriteAutoPropertyDeclaration("public", attribute.TypeName, attribute.PropertyName);
            }
        }

        private void BuildProcessMethodString(TagHelperDescriptor descriptor)
        {
            var contextVariable = "context";
            var outputVariable = "output";

            using (Writer.BuildMethodDeclaration(
                    $"public override async",
                    nameof(System.Threading.Tasks.Task),
                    "ProcessAsync",
                    new Dictionary<string, string>()
                    {
                        { $"{ _context.TagHelpersNamespace }.{nameof(TagHelperContext)}", contextVariable },
                        { $"{ _context.TagHelpersNamespace }.{nameof(TagHelperOutput)}", outputVariable }
                    }))
            {

                Writer.WriteInstanceMethodInvocation(
                    $"(({_context.IViewContextAwareType})_viewComponentHelper)",
                    "Contextualize",
                    new string[] { _context.ViewContextType });

                var viewComponentName = ViewComponentTagHelperDescriptorConventions.GetViewComponentName(descriptor);
                var viewComponentParameters = GetParametersObjectString(descriptor);
                var methodParameters = new string[] { $"\"{viewComponentName}\"", viewComponentParameters };

                var viewContentVariable = "viewContent";
                Writer.Write("var")
                    .Write(" ")
                    .WriteStartAssignment(viewContentVariable)
                    .WriteInstanceMethodInvocation("await _viewComponentHelper", "InvokeAsync", methodParameters);

                Writer.WriteVariableDeclaration("", $"{outputVariable}.TagName", "");
                Writer.WriteInstanceMethodInvocation(
                    $"{outputVariable}.Content",
                    "SetHtmlContent",
                    new string[] { viewContentVariable });
            }
        }

        private string GetParametersObjectString(TagHelperDescriptor descriptor)
        {
            var propertyNames = descriptor.Attributes.Select(attribute => attribute.PropertyName);
            var joinedPropertyNames = string.Join(", ", propertyNames);
            var parametersString = $" new {{ { joinedPropertyNames } }}";

            return parametersString;
        }

        private void BuildTargetElementString(TagHelperDescriptor descriptor)
        {
            var selfClosingTagStructure = nameof(TagStructure.NormalOrSelfClosing);
            var tagStructure = $"{_context.TagHelpersNamespace}.{_context.TagStructureType}.{selfClosingTagStructure}";

            Writer.Write("[")
                .WriteStartMethodInvocation(typeof(HtmlTargetElementAttribute).FullName)
                .WriteStringLiteral(descriptor.FullTagName)
                .WriteParameterSeparator()
                .WriteStartAssignment(_context.TagStructureType)
                .Write(tagStructure)
                .WriteEndMethodInvocation(endLine: false)
                .Write("]")
                .WriteLine();
        }
    }
}