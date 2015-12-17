using System;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IModelBindingContext
    {
        string BinderModelName { get; set; }
        Type BinderType { get; set; }
        BindingSource BindingSource { get; set; }
        bool FallbackToEmptyPrefix { get; set; }
        string FieldName { get; set; }
        bool IsTopLevelObject { get; set; }
        object Model { get; set; }
        ModelMetadata ModelMetadata { get; set; }
        string ModelName { get; set; }
        ModelStateDictionary ModelState { get; set; }
        Type ModelType { get; }
        OperationBindingContext OperationBindingContext { get; set; }
        Func<IModelBindingContext, string, bool> PropertyFilter { get; set; }
        ValidationStateDictionary ValidationState { get; set; }
        IValueProvider ValueProvider { get; set; }

        ModelBindingResult? Result { get; set; }

        ModelBindingContextDisposable PushContext(ModelMetadata modelMetadata, string fieldName, string modelName, object model);
        ModelBindingContextDisposable PushContext();
        void PopContext();
    }

    public struct ModelBindingContextDisposable : IDisposable
    {
        private readonly IModelBindingContext _context;
        public ModelBindingContextDisposable(IModelBindingContext context)
        {
            _context = context;
        }
        public void Dispose()
        {
            _context.PopContext();
        }
    }
}
