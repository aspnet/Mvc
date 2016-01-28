// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
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
}
