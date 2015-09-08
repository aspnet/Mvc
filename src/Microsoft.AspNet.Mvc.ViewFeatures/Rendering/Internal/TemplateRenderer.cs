// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering.Internal
{
    public class TemplateRenderer
    {
        private const string DisplayTemplateViewPath = "DisplayTemplates";
        private const string EditorTemplateViewPath = "EditorTemplates";
        public const string IEnumerableOfIFormFileName = "IEnumerable`" + nameof(IFormFile);

        private static readonly Dictionary<string, Func<IHtmlHelper, IHtmlContent>> _defaultDisplayActions =
            new Dictionary<string, Func<IHtmlHelper, IHtmlContent>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Collection", DefaultDisplayTemplates.CollectionTemplate },
                { "EmailAddress", DefaultDisplayTemplates.EmailAddressTemplate },
                { "HiddenInput", DefaultDisplayTemplates.HiddenInputTemplate },
                { "Html", DefaultDisplayTemplates.HtmlTemplate },
                { "Text", DefaultDisplayTemplates.StringTemplate },
                { "Url", DefaultDisplayTemplates.UrlTemplate },
                { typeof(bool).Name, DefaultDisplayTemplates.BooleanTemplate },
                { typeof(decimal).Name, DefaultDisplayTemplates.DecimalTemplate },
                { typeof(string).Name, DefaultDisplayTemplates.StringTemplate },
                { typeof(object).Name, DefaultDisplayTemplates.ObjectTemplate },
            };

        private static readonly Dictionary<string, Func<IHtmlHelper, IHtmlContent>> _defaultEditorActions =
            new Dictionary<string, Func<IHtmlHelper, IHtmlContent>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Collection", DefaultEditorTemplates.CollectionTemplate },
                { "EmailAddress", DefaultEditorTemplates.EmailAddressInputTemplate },
                { "HiddenInput", DefaultEditorTemplates.HiddenInputTemplate },
                { "MultilineText", DefaultEditorTemplates.MultilineTemplate },
                { "Password", DefaultEditorTemplates.PasswordTemplate },
                { "PhoneNumber", DefaultEditorTemplates.PhoneNumberInputTemplate },
                { "Text", DefaultEditorTemplates.StringTemplate },
                { "Url", DefaultEditorTemplates.UrlInputTemplate },
                { "Date", DefaultEditorTemplates.DateInputTemplate },
                { "DateTime", DefaultEditorTemplates.DateTimeInputTemplate },
                { "DateTime-local", DefaultEditorTemplates.DateTimeLocalInputTemplate },
                { "Time", DefaultEditorTemplates.TimeInputTemplate },
                { typeof(byte).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(sbyte).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(short).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(ushort).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(int).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(uint).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(long).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(ulong).Name, DefaultEditorTemplates.NumberInputTemplate },
                { typeof(bool).Name, DefaultEditorTemplates.BooleanTemplate },
                { typeof(decimal).Name, DefaultEditorTemplates.DecimalTemplate },
                { typeof(string).Name, DefaultEditorTemplates.StringTemplate },
                { typeof(object).Name, DefaultEditorTemplates.ObjectTemplate },
                { typeof(IFormFile).Name, DefaultEditorTemplates.FileInputTemplate },
                { IEnumerableOfIFormFileName, DefaultEditorTemplates.FileCollectionInputTemplate },
            };

        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private IViewEngine _viewEngine;
        private string _templateName;
        private bool _readOnly;

        public TemplateRenderer(
            [NotNull] IViewEngine viewEngine,
            [NotNull] ViewContext viewContext,
            [NotNull] ViewDataDictionary viewData,
            string templateName,
            bool readOnly)
        {
            _viewEngine = viewEngine;
            _viewContext = viewContext;
            _viewData = viewData;
            _templateName = templateName;
            _readOnly = readOnly;
        }

        public IHtmlContent Render()
        {
            var defaultActions = GetDefaultActions();
            var modeViewPath = _readOnly ? DisplayTemplateViewPath : EditorTemplateViewPath;

            foreach (string viewName in GetViewNames())
            {
                var fullViewName = modeViewPath + "/" + viewName;

                var viewEngineResult = _viewEngine.FindPartialView(_viewContext, fullViewName);
                if (viewEngineResult.Success)
                {
                    using (var writer = new StringCollectionTextWriter(_viewContext.Writer.Encoding))
                    {
                        // Forcing synchronous behavior so users don't have to await templates.
                        var view = viewEngineResult.View;
                        using (view as IDisposable)
                        {
                            var viewContext = new ViewContext(_viewContext, viewEngineResult.View, _viewData, writer);
                            var renderTask = viewEngineResult.View.RenderAsync(viewContext);
                            renderTask.GetAwaiter().GetResult();
                            return writer.Content;
                        }
                    }
                }

                Func<IHtmlHelper, IHtmlContent> defaultAction;
                if (defaultActions.TryGetValue(viewName, out defaultAction))
                {
                    return defaultAction(MakeHtmlHelper(_viewContext, _viewData));
                }
            }

            throw new InvalidOperationException(
                Resources.FormatTemplateHelpers_NoTemplate(_viewData.ModelExplorer.ModelType.FullName));
        }

        private Dictionary<string, Func<IHtmlHelper, IHtmlContent>> GetDefaultActions()
        {
            return _readOnly ? _defaultDisplayActions : _defaultEditorActions;
        }

        private IEnumerable<string> GetViewNames()
        {
            var metadata = _viewData.ModelMetadata;
            var templateHints = new string[]
            {
                _templateName,
                metadata.TemplateHint,
                metadata.DataTypeName
            };

            foreach (var templateHint in templateHints.Where(s => !string.IsNullOrEmpty(s)))
            {
                yield return templateHint;
            }

            // We don't want to search for Nullable<T>, we want to search for T (which should handle both T and
            // Nullable<T>).
            var fieldType = metadata.UnderlyingOrModelType;
            foreach (var typeName in GetTypeNames(metadata, fieldType))
            {
                yield return typeName;
            }
        }

        public static IEnumerable<string> GetTypeNames(ModelMetadata modelMetadata, Type fieldType)
        {
            // Not returning type name here for IEnumerable<IFormFile> since we will be returning
            // a more specific name, IEnumerableOfIFormFileName.
            var fieldTypeInfo = fieldType.GetTypeInfo();

            if (typeof(IEnumerable<IFormFile>) != fieldType)
            {
                yield return fieldType.Name;
            }

            if (fieldType == typeof(string))
            {
                // Nothing more to provide
                yield break;
            }
            else if (!modelMetadata.IsComplexType)
            {
                // IsEnum is false for the Enum class itself
                if (fieldTypeInfo.IsEnum)
                {
                    // Same as fieldType.BaseType.Name in this case
                    yield return "Enum";
                }
                else if (fieldType == typeof(DateTimeOffset))
                {
                    yield return "DateTime";
                }

                yield return "String";
                yield break;
            }
            else if (!fieldTypeInfo.IsInterface)
            {
                var type = fieldType;
                while (true)
                {
                    type = type.GetTypeInfo().BaseType;
                    if (type == null || type == typeof(object))
                    {
                        break;
                    }

                    yield return type.Name;
                }
            }

            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(fieldTypeInfo))
            {
                if (typeof(IEnumerable<IFormFile>).GetTypeInfo().IsAssignableFrom(fieldTypeInfo))
                {
                    yield return IEnumerableOfIFormFileName;

                    // Specific name has already been returned, now return the generic name.
                    if (typeof(IEnumerable<IFormFile>) == fieldType)
                    {
                        yield return fieldType.Name;
                    }
                }

                yield return "Collection";
            }
            else if (typeof(IFormFile) != fieldType && typeof(IFormFile).GetTypeInfo().IsAssignableFrom(fieldTypeInfo))
            {
                yield return nameof(IFormFile);
            }

            yield return "Object";
        }

        private static IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            var contextable = newHelper as ICanHasViewContext;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}
