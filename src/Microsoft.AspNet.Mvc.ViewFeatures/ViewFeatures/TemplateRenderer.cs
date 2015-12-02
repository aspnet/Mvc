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
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ViewFeatures.Internal
{
    public class TemplateRenderer
    {
        private const string DisplayTemplateViewPath = "DisplayTemplates";
        private const string EditorTemplateViewPath = "EditorTemplates";
        public const string IEnumerableOfIFormFileName = "IEnumerable`" + nameof(IFormFile);


        private static Dictionary<string, Func<IHtmlHelper, IHtmlContent>> _defaultDisplayActions;

        private static Dictionary<string, Func<IHtmlHelper, IHtmlContent>> InitDefaultDisplayActions(IDefaultDisplayTemplates defaultDisplayTemplates)
        {
            return
            new Dictionary<string, Func<IHtmlHelper, IHtmlContent>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Collection", defaultDisplayTemplates.CollectionTemplate },
                { "EmailAddress", defaultDisplayTemplates.EmailAddressTemplate },
                { "HiddenInput", defaultDisplayTemplates.HiddenInputTemplate },
                { "Html", defaultDisplayTemplates.HtmlTemplate },
                { "Text", defaultDisplayTemplates.StringTemplate },
                { "Url", defaultDisplayTemplates.UrlTemplate },
                { typeof(bool).Name, defaultDisplayTemplates.BooleanTemplate },
                { typeof(decimal).Name, defaultDisplayTemplates.DecimalTemplate },
                { typeof(string).Name, defaultDisplayTemplates.StringTemplate },
                { typeof(object).Name, defaultDisplayTemplates.ObjectTemplate },
            };
        }
        private static Dictionary<string, Func<IHtmlHelper, IHtmlContent>> _defaultEditorActions;

        private static Dictionary<string, Func<IHtmlHelper, IHtmlContent>> InitDefaultEditorActions(IDefaultEditorTemplates defaultEditorTemplates)
        { 
            return
            new Dictionary<string, Func<IHtmlHelper, IHtmlContent>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Collection", defaultEditorTemplates.CollectionTemplate },
                { "EmailAddress", defaultEditorTemplates.EmailAddressInputTemplate },
                { "HiddenInput", defaultEditorTemplates.HiddenInputTemplate },
                { "MultilineText", defaultEditorTemplates.MultilineTemplate },
                { "Password", defaultEditorTemplates.PasswordTemplate },
                { "PhoneNumber", defaultEditorTemplates.PhoneNumberInputTemplate },
                { "Text", defaultEditorTemplates.StringTemplate },
                { "Url", defaultEditorTemplates.UrlInputTemplate },
                { "Date", defaultEditorTemplates.DateInputTemplate },
                { "DateTime", defaultEditorTemplates.DateTimeInputTemplate },
                { "DateTime-local", defaultEditorTemplates.DateTimeLocalInputTemplate },
                { "Time", defaultEditorTemplates.TimeInputTemplate },
                { typeof(byte).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(sbyte).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(short).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(ushort).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(int).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(uint).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(long).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(ulong).Name, defaultEditorTemplates.NumberInputTemplate },
                { typeof(bool).Name, defaultEditorTemplates.BooleanTemplate },
                { typeof(decimal).Name, defaultEditorTemplates.DecimalTemplate },
                { typeof(string).Name, defaultEditorTemplates.StringTemplate },
                { typeof(object).Name, defaultEditorTemplates.ObjectTemplate },
                { typeof(IFormFile).Name, defaultEditorTemplates.FileInputTemplate },
                { IEnumerableOfIFormFileName, defaultEditorTemplates.FileCollectionInputTemplate },
            };
    }

        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private IViewEngine _viewEngine;
        private string _templateName;
        private bool _readOnly;

        public TemplateRenderer(
            IViewEngine viewEngine,
            ViewContext viewContext,
            ViewDataDictionary viewData,
            string templateName,
            bool readOnly)
        {
            if (viewEngine == null)
            {
                throw new ArgumentNullException(nameof(viewEngine));
            }

            if (viewContext == null)
            {
                throw new ArgumentNullException(nameof(viewContext));
            }

            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

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
                var viewEngineResult = _viewEngine.GetView(_viewContext.ExecutingFilePath, viewName, isMainPage: false);
                if (!viewEngineResult.Success)
                {
                    var fullViewName = modeViewPath + "/" + viewName;
                    viewEngineResult = _viewEngine.FindView(_viewContext, fullViewName, isMainPage: false);
                }

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
            if (_readOnly) {
                if (_defaultDisplayActions == null)
                {
                    var defaultDisplayTemplates = _viewContext.HttpContext.RequestServices.GetRequiredService<IDefaultDisplayTemplates>();
                    _defaultDisplayActions = InitDefaultDisplayActions(defaultDisplayTemplates);
                }
                return _defaultDisplayActions;
            }
            else
            {
                if (_defaultEditorActions == null)
                {
                    var defaultEditorTemplates = _viewContext.HttpContext.RequestServices.GetRequiredService<IDefaultEditorTemplates>();
                    _defaultEditorActions = InitDefaultEditorActions(defaultEditorTemplates);
                }
                return _defaultEditorActions;
            }
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
