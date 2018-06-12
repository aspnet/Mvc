using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public readonly struct RazorPageViewDataDictionaryFactory
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly Func<IModelMetadataProvider, ModelStateDictionary, ViewDataDictionary> _rootFactory;
        private readonly Func<ViewDataDictionary, ViewDataDictionary> _nestedFactory;
        private readonly Type _viewDataDictionaryType;

        public RazorPageViewDataDictionaryFactory(IModelMetadataProvider metadataProvider, Type declaredModelType)
        {
            _metadataProvider = metadataProvider;

            // In the absence of a model on the current type, we'll attempt to use ViewDataDictionary<object> on the current type.
            var viewDataDictionaryModelType = declaredModelType ?? typeof(object);

            _viewDataDictionaryType = typeof(ViewDataDictionary<>).MakeGenericType(viewDataDictionaryModelType);
            _rootFactory = ViewDataDictionaryFactory.CreateFactory(viewDataDictionaryModelType.GetTypeInfo());
            _nestedFactory = ViewDataDictionaryFactory.CreateNestedFactory(viewDataDictionaryModelType.GetTypeInfo());
        }

        public ViewDataDictionary CreateViewDataDictionary(ViewContext context)
        {
            // Create a ViewDataDictionary<TModel> if the ViewContext.ViewData is not set or the type of
            // ViewContext.ViewData is an incompatible type.
            if (context.ViewData == null)
            {
                // Create ViewDataDictionary<TModel>(IModelMetadataProvider, ModelStateDictionary).
                return _rootFactory(_metadataProvider, context.ModelState);
            }
            else if (context.ViewData.GetType() != _viewDataDictionaryType)
            {
                // Create ViewDataDictionary<TModel>(ViewDataDictionary).
                return _nestedFactory(context.ViewData);
            }

            return context.ViewData;
        }
    }
}
