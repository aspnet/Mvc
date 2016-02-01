// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains operating information for model binding and validation.
    /// </summary>
    public class ModelBindingContext : IModelBindingContext
    {
        State _state;
        Stack<State> _stack = new Stack<State>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingContext"/> class.
        /// </summary>
        public ModelBindingContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ModelBindingContext"/> for top-level model binding operation.
        /// </summary>
        /// <param name="operationBindingContext">
        /// The <see cref="OperationBindingContext"/> associated with the binding operation.
        /// </param>
        /// <param name="metadata"><see cref="ModelMetadata"/> associated with the model.</param>
        /// <param name="bindingInfo"><see cref="BindingInfo"/> associated with the model.</param>
        /// <param name="modelName">The name of the property or parameter being bound.</param>
        /// <returns>A new instance of <see cref="ModelBindingContext"/>.</returns>
        public static IModelBindingContext CreateBindingContext(
            OperationBindingContext operationBindingContext,
            ModelMetadata metadata,
            BindingInfo bindingInfo,
            string modelName)
        {
            if (operationBindingContext == null)
            {
                throw new ArgumentNullException(nameof(operationBindingContext));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (modelName == null)
            {
                throw new ArgumentNullException(nameof(modelName));
            }

            var binderModelName = bindingInfo?.BinderModelName ?? metadata.BinderModelName;
            var propertyPredicateProvider =
                bindingInfo?.PropertyBindingPredicateProvider ?? metadata.PropertyBindingPredicateProvider;

            return new ModelBindingContext()
            {
                BinderModelName = binderModelName,
                BindingSource = bindingInfo?.BindingSource ?? metadata.BindingSource,
                BinderType = bindingInfo?.BinderType ?? metadata.BinderType,
                PropertyFilter = propertyPredicateProvider?.PropertyFilter,

                // We only support fallback to empty prefix in cases where the model name is inferred from
                // the parameter or property being bound.
                FallbackToEmptyPrefix = binderModelName == null,

                // Because this is the top-level context, FieldName and ModelName should be the same.
                FieldName = binderModelName ?? modelName,
                ModelName = binderModelName ?? modelName,

                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelState = operationBindingContext.ActionContext.ModelState,
                OperationBindingContext = operationBindingContext,
                ValueProvider = operationBindingContext.ValueProvider,

                ValidationState = new ValidationStateDictionary(),
            };
        }

        public ModelBindingContextDisposable PushContext(
            ModelMetadata modelMetadata,
            string fieldName,
            string modelName,
            object model)
        {
            if (modelMetadata == null)
            {
                throw new ArgumentNullException(nameof(modelMetadata));
            }

            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            if (modelName == null)
            {
                throw new ArgumentNullException(nameof(modelName));
            }

            var scope = PushContext();

            Model = model;
            ModelMetadata = modelMetadata;
            ModelName = modelName;
            FieldName = fieldName;
            BinderModelName = modelMetadata.BinderModelName;
            BinderType = modelMetadata.BinderType;
            BindingSource = modelMetadata.BindingSource;
            PropertyFilter = modelMetadata.PropertyBindingPredicateProvider?.PropertyFilter;

            FallbackToEmptyPrefix = false;
            IsTopLevelObject = false;

            return scope;
        }

        public ModelBindingContextDisposable PushContext()
        {
            _stack.Push(_state);
            _state.Result = null;
            return new ModelBindingContextDisposable(this);
        }

        public void PopContext()
        {
            _state = _stack.Pop();
        }

        /// <summary>
        /// Represents the <see cref="OperationBindingContext"/> associated with this context.
        /// </summary>
        public OperationBindingContext OperationBindingContext
        {
            get { return _state.OperationBindingContext; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.OperationBindingContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the current field being bound.
        /// </summary>
        public string FieldName
        {
            get { return _state.FieldName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.FieldName = value;
            }
        }

        /// <summary>
        /// Gets or sets the model value for the current operation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Model"/> will typically be set for a binding operation that works
        /// against a pre-existing model object to update certain properties.
        /// </remarks>
        public object Model { get { return _state.Model; } set { _state.Model = value; } }

        /// <summary>
        /// Gets or sets the metadata for the model associated with this context.
        /// </summary>
        public ModelMetadata ModelMetadata
        {
            get { return _state.ModelMetadata; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.ModelMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the model. This property is used as a key for looking up values in
        /// <see cref="IValueProvider"/> during model binding.
        /// </summary>
        public string ModelName
        {
            get { return _state.ModelName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.ModelName = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ModelStateDictionary"/> used to capture <see cref="ModelState"/> values
        /// for properties in the object graph of the model when binding.
        /// </summary>
        public ModelStateDictionary ModelState
        {
            get { return _state.ModelState; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.ModelState = value;
            }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <remarks>
        /// The <see cref="ModelMetadata"/> property must be set to access this property.
        /// </remarks>
        public Type ModelType => ModelMetadata?.ModelType;

        /// <summary>
        /// Gets or sets a model name which is explicitly set using an <see cref="IModelNameProvider"/>.
        /// <see cref="Model"/>.
        /// </summary>
        public string BinderModelName { get { return _state.BinderModelName; } set { _state.BinderModelName = value; } }

        /// <summary>
        /// Gets or sets a value which represents the <see cref="BindingSource"/> associated with the
        /// <see cref="Model"/>.
        /// </summary>
        public BindingSource BindingSource { get { return _state.BindingSource; } set { _state.BindingSource = value; } }

        /// <summary>
        /// Gets the <see cref="Type"/> of an <see cref="IModelBinder"/> associated with the
        /// <see cref="Model"/>.
        /// </summary>
        public Type BinderType { get { return _state.BinderType; } set { _state.BinderType = value; } }

        /// <summary>
        /// Gets or sets a value that indicates whether the binder should use an empty prefix to look up
        /// values in <see cref="IValueProvider"/> when no values are found using the <see cref="ModelName"/> prefix.
        /// </summary>
        /// <remarks>
        /// Passed into the model binding system. Should not be <c>true</c> when <see cref="IsTopLevelObject"/> is
        /// <c>false</c>.
        /// </remarks>
        public bool FallbackToEmptyPrefix { get { return _state.FallbackToEmptyPrefix; } set { _state.FallbackToEmptyPrefix = value; } }

        /// <summary>
        /// Gets or sets an indication that the current binder is handling the top-level object.
        /// </summary>
        /// <remarks>Passed into the model binding system.</remarks>
        public bool IsTopLevelObject { get { return _state.IsTopLevelObject; } set { _state.IsTopLevelObject = value; } }

        /// <summary>
        /// Gets or sets the <see cref="IValueProvider"/> associated with this context.
        /// </summary>
        public IValueProvider ValueProvider
        {
            get { return _state.ValueProvider; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _state.ValueProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets a predicate which will be evaluated for each property to determine if the property
        /// is eligible for model binding.
        /// </summary>
        public Func<IModelBindingContext, string, bool> PropertyFilter { get { return _state.PropertyFilter; } set { _state.PropertyFilter = value; } }

        /// <summary>
        /// Gets or sets the <see cref="ValidationStateDictionary"/>. Used for tracking validation state to
        /// customize validation behavior for a model object.
        /// </summary>
        public ValidationStateDictionary ValidationState { get { return _state.ValidationState; } set { _state.ValidationState = value; } }

        public ModelBindingResult? Result
        {
            get
            {
                return _state.Result;
            }
            set
            {
#if DEBUG
                if (value.HasValue && value.Value == default(ModelBindingResult))
                {
                    throw new ArgumentException(nameof(ModelBindingResult));
                }
#endif

                _state.Result = value;
            }
        }


        private struct State
        {
            public OperationBindingContext OperationBindingContext;
            public string FieldName;
            public object Model;
            public ModelMetadata ModelMetadata;
            public string ModelName;

            public IValueProvider ValueProvider;
            public Func<IModelBindingContext, string, bool> PropertyFilter;
            public ValidationStateDictionary ValidationState;
            public ModelStateDictionary ModelState;

            public string BinderModelName;
            public BindingSource BindingSource;
            public Type BinderType;
            public bool FallbackToEmptyPrefix;
            public bool IsTopLevelObject;

            public ModelBindingResult? Result;
        };
    }
}
