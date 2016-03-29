﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains operating information for model binding and validation.
    /// </summary>
    public abstract class ModelBindingContext
    {
        /// <summary>
        /// Gets or sets a model name which is explicitly set using an <see cref="IModelNameProvider"/>.
        /// </summary>
        public abstract string BinderModelName { get; set; }

        /// <summary>
        /// Gets or sets a value which represents the <see cref="ModelBinding.BindingSource"/> associated with the
        /// <see cref="Model"/>.
        /// </summary>
        public abstract BindingSource BindingSource { get; set; }

        /// <summary>
        /// Gets or sets the name of the current field being bound.
        /// </summary>
        public abstract string FieldName { get; set; }

        /// <summary>
        /// Gets or sets an indication that the current binder is handling the top-level object.
        /// </summary>
        /// <remarks>Passed into the model binding system.</remarks>
        public abstract bool IsTopLevelObject { get; set; }

        /// <summary>
        /// Gets or sets the model value for the current operation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Model"/> will typically be set for a binding operation that works
        /// against a pre-existing model object to update certain properties.
        /// </remarks>
        public abstract object Model { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the model associated with this context.
        /// </summary>
        public abstract ModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name of the model. This property is used as a key for looking up values in
        /// <see cref="IValueProvider"/> during model binding.
        /// </summary>
        public abstract string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ModelStateDictionary"/> used to capture <see cref="ModelState"/> values
        /// for properties in the object graph of the model when binding.
        /// </summary>
        public abstract ModelStateDictionary ModelState { get; set; }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <remarks>
        /// The <see cref="ModelMetadata"/> property must be set to access this property.
        /// </remarks>
        public abstract Type ModelType { get; }

        /// <summary>
        /// Represents the <see cref="ModelBinding.OperationBindingContext"/> associated with this context.
        /// </summary>
        public abstract OperationBindingContext OperationBindingContext { get; set; }

        /// <summary>
        /// Gets or sets a predicate which will be evaluated for each property to determine if the property
        /// is eligible for model binding.
        /// </summary>
        public abstract Func<ModelBindingContext, string, bool> PropertyFilter { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ValidationStateDictionary"/>. Used for tracking validation state to
        /// customize validation behavior for a model object.
        /// </summary>
        public abstract ValidationStateDictionary ValidationState { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueProvider"/> associated with this context.
        /// </summary>
        public abstract IValueProvider ValueProvider { get; set; }

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
        public abstract ModelBindingResult? Result { get; set; }

        /// <summary>
        /// Pushes a layer of state onto this context. Model binders will call this as part of recursion when binding properties
        /// or collection items.
        /// </summary>
        /// <param name="modelMetadata"><see cref="ModelBinding.ModelMetadata"/> to assign to the <see cref="ModelMetadata"/> property.</param>
        /// <param name="fieldName">Name to assign to the <see cref="FieldName"/> property.</param>
        /// <param name="modelName">Name to assign to the <see cref="ModelName"/> property.</param>
        /// <param name="model">Instance to assign to the <see cref="Model"/> property.</param>
        /// <returns>A <see cref="NestedScope"/> scope object which should be used in a using statement where PushContext is called.</returns>
        public abstract NestedScope EnterNestedScope(ModelMetadata modelMetadata, string fieldName, string modelName, object model);

        /// <summary>
        /// Pushes a layer of state onto this context. Model binders will call this as part of recursion when binding properties
        /// or collection items.
        /// </summary>
        /// <returns>A <see cref="NestedScope"/> scope object which should be used in a using statement where PushContext is called.</returns>        
        public abstract NestedScope EnterNestedScope();

        /// <summary>
        /// Removes a layer of state pushed by calling <see cref="M:EnterNestedScope"/>.
        /// </summary>
        protected abstract void ExitNestedScope();

        /// <summary>
        /// Return value of <see cref="M:EnterNestedScope"/>. Should be disposed
        /// by caller when child binding context state should be popped off of 
        /// the <see cref="ModelBindingContext"/>.
        /// </summary>
        public struct NestedScope : IDisposable
        {
            private readonly ModelBindingContext _context;

            /// <summary>
            /// Initializes the <see cref="NestedScope"/> for a <see cref="ModelBindingContext"/>.
            /// </summary>
            /// <param name="context"></param>
            public NestedScope(ModelBindingContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Exits the <see cref="NestedScope"/> created by calling <see cref="M:EnterNestedScope"/>.
            /// </summary>
            public void Dispose()
            {
                _context.ExitNestedScope();
            }
        }
    }
}
