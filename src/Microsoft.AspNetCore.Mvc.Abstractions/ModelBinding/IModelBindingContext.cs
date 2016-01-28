// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains operating information for model binding and validation.
    /// </summary>
    public interface IModelBindingContext
    {
        /// <summary>
        /// Gets or sets a model name which is explicitly set using an <see cref="IModelNameProvider"/>.
        /// <see cref="Model"/>.
        /// </summary>
        string BinderModelName { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of an <see cref="IModelBinder"/> associated with the
        /// <see cref="Model"/>.
        /// </summary>
        Type BinderType { get; set; }

        /// <summary>
        /// Gets or sets a value which represents the <see cref="BindingSource"/> associated with the
        /// <see cref="Model"/>.
        /// </summary>
        BindingSource BindingSource { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the binder should use an empty prefix to look up
        /// values in <see cref="IValueProvider"/> when no values are found using the <see cref="ModelName"/> prefix.
        /// </summary>
        /// <remarks>
        /// Passed into the model binding system. Should not be <c>true</c> when <see cref="IsTopLevelObject"/> is
        /// <c>false</c>.
        /// </remarks>
        bool FallbackToEmptyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the name of the current field being bound.
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// Gets or sets an indication that the current binder is handling the top-level object.
        /// </summary>
        /// <remarks>Passed into the model binding system.</remarks>
        bool IsTopLevelObject { get; set; }

        /// <summary>
        /// Gets or sets the model value for the current operation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Model"/> will typically be set for a binding operation that works
        /// against a pre-existing model object to update certain properties.
        /// </remarks>
        object Model { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the model associated with this context.
        /// </summary>
        ModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name of the model. This property is used as a key for looking up values in
        /// <see cref="IValueProvider"/> during model binding.
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ModelStateDictionary"/> used to capture <see cref="ModelState"/> values
        /// for properties in the object graph of the model when binding.
        /// </summary>
        ModelStateDictionary ModelState { get; set; }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <remarks>
        /// The <see cref="ModelMetadata"/> property must be set to access this property.
        /// </remarks>
        Type ModelType { get; }

        /// <summary>
        /// Represents the <see cref="OperationBindingContext"/> associated with this context.
        /// </summary>
        OperationBindingContext OperationBindingContext { get; set; }

        /// <summary>
        /// Gets or sets a predicate which will be evaluated for each property to determine if the property
        /// is eligible for model binding.
        /// </summary>
        Func<IModelBindingContext, string, bool> PropertyFilter { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ValidationStateDictionary"/>. Used for tracking validation state to
        /// customize validation behavior for a model object.
        /// </summary>
        ValidationStateDictionary ValidationState { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueProvider"/> associated with this context.
        /// </summary>
        IValueProvider ValueProvider { get; set; }

        /// <summary>
        /// <para>
        /// On completion returns a <see cref="ModelBindingResult"/> which
        /// represents the result of the model binding process.
        /// </para>
        /// <para>
        /// If model binding was successful, the <see cref="ModelBindingResult"/> should be a value created
        /// with <see cref="ModelBindingResult.Success"/>. If model binding failed, the
        /// <see cref="ModelBindingResult"/> should be a value created with <see cref="ModelBindingResult.Failed"/>.
        /// If there was no data, or this model binder cannot handle the operation, the
        /// <see cref="ModelBindingResult"/> should be null.
        /// </para>
        /// </summary>
        ModelBindingResult? Result { get; set; }

        /// <summary>
        /// Pushes a layer of state onto this context. Model binders will call this as part of recursion when binding properties
        /// or collection items.
        /// </summary>
        /// <param name="modelMetadata"><see cref="ModelMetadata"/> to assign to the <see cref="IModelBindingContext.ModelMetadata"/> property</param>
        /// <param name="fieldName">Value to assign to the <see cref="IModelBindingContext.FieldName"/> property</param>
        /// <param name="modelName"><see cref="IModelBindingContext.ModelName"/> property</param>
        /// <param name="model"><see cref="IModelBindingContext.Model"/> property</param>
        /// <returns>A <see cref="ModelBindingContextDisposable"/> scope object which should be used in a using statement where PushContext is called. 
        /// <see cref="PopContext"/> must not be called if this is done.</returns>
        ModelBindingContextDisposable PushContext(ModelMetadata modelMetadata, string fieldName, string modelName, object model);

        /// <summary>
        /// Pushes a layer of state onto this context. Model binders will call this as part of recursion when binding properties
        /// or collection items.
        /// </summary>
        /// <returns>A <see cref="ModelBindingContextDisposable"/> scope object which should be used in a using statement where PushContext is called. 
        /// <see cref="PopContext"/> must not be called if this is done.</returns>        
        ModelBindingContextDisposable PushContext();

        /// <summary>
        /// Removes a layer of state pushed by calling <see cref="PushContext"/>.
        /// </summary>
        void PopContext();
    }
}
