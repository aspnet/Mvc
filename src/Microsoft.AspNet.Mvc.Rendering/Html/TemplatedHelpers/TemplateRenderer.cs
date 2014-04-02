using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class TemplateRenderer
    {
        private static readonly string DisplayTemplateViewPath = "DisplayTemplates";
        private static readonly string EditorTemplateViewPath = "EditorTemplates";

        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private string _templateName;
        private bool _readOnly;

        public TemplateRenderer(ViewContext viewContext, ViewDataDictionary viewData, string templateName, bool readOnly)
        {
            _viewContext = viewContext;
            _viewData = viewData;
            _templateName = templateName;
            _readOnly = readOnly;
        }

        public string Render()
        {
            var actionCache = ActionCacheProvider.GetActionCacheItem(_viewContext.HttpContext);
            var defaultActions = GetDefaultActions();
            string modeViewPath = _readOnly ? DisplayTemplateViewPath : EditorTemplateViewPath;

            foreach (string viewName in GetViewNames())
            {
                string fullViewName = modeViewPath + "/" + viewName;
                ActionCacheItem cacheItem;

                if (actionCache.TryGetValue(fullViewName, out cacheItem))
                {
                    if (cacheItem != null)
                    {
                        // Forcing synchronous behavior so users don't have to await templates.
                        return cacheItem.Execute(_viewContext, _viewData).Result;
                    }
                }
                else
                {
                    var viewEngine = _viewContext.ServiceProvider.GetService<IViewEngine>();
                    // Forcing synchronous behavior so users don't have to await templates.
                    var viewEngineResult = viewEngine.FindPartialView(_viewContext.ViewEngineContext, fullViewName).Result;
                    if (viewEngineResult.View != null)
                    {
                        actionCache[fullViewName] = new ActionCacheViewItem { ViewName = fullViewName };

                        using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                        {
                            // Forcing synchronous behavior so users don't have to await templates.
                            // TODO: Pass through TempData once implemented.
                            viewEngineResult.View.RenderAsync(new ViewContext(_viewContext)
                            {
                                ViewData = _viewData,
                                Writer = writer,
                            }).Wait();

                            return writer.ToString();
                        }
                    }

                    Func<ViewContext, Task<string>> defaultAction;
                    if (defaultActions.TryGetValue(viewName, out defaultAction))
                    {
                        actionCache[fullViewName] = new ActionCacheCodeItem { Action = defaultAction };
                        // Forcing synchronous behavior so users don't have to await templates.
                        return defaultAction(_viewContext).Result;
                    }

                    actionCache[fullViewName] = null;
                }
            }

            throw new InvalidOperationException(Resources.FormatTemplateHelpers_NoTemplate(_viewData.ModelMetadata.GetRealModelType().FullName));
        }

        private Dictionary<string, Func<ViewContext, Task<string>>> GetDefaultActions()
        {
            // TODO: Implement default templates
            return new Dictionary<string, Func<ViewContext, Task<string>>>(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetViewNames()
        {
            var metadata = _viewData.ModelMetadata;
            var templateHints = new string[] {
                _templateName, 
                metadata.TemplateHint, 
                metadata.DataTypeName
            };

            foreach (string templateHint in templateHints.Where(s => !string.IsNullOrEmpty(s)))
            {
                yield return templateHint;
            }

            // We don't want to search for Nullable<T>, we want to search for T (which should handle both T and Nullable<T>)
            var fieldType = Nullable.GetUnderlyingType(metadata.GetRealModelType()) ?? metadata.GetRealModelType();

            yield return fieldType.Name;

            if (fieldType == typeof(string))
            {
                // Nothing more to provide
                yield break;
            }
            else if (!metadata.IsComplexType)
            {
                // IsEnum is false for the Enum class itself
                if (fieldType.IsEnum)
                {
                    // Same as fieldType.BaseType.Name in this case
                    yield return "Enum";
                }
                else if (fieldType == typeof(DateTimeOffset))
                {
                    yield return "DateTime";
                }

                yield return "String";
            }
            else if (fieldType.IsInterface)
            {
                if (typeof(IEnumerable).IsAssignableFrom(fieldType))
                {
                    yield return "Collection";
                }

                yield return "Object";
            }
            else
            {
                bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(fieldType);

                while (true)
                {
                    fieldType = fieldType.BaseType;
                    if (fieldType == null)
                    {
                        break;
                    }

                    if (isEnumerable && fieldType == typeof(Object))
                    {
                        yield return "Collection";
                    }

                    yield return fieldType.Name;
                }
            }
        }
    }
}
