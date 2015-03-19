// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains operating information for model binding and validation.
    /// </summary>
    public class ModelBindingContext
    {
        private static readonly Func<ModelBindingContext, string, bool>
            _defaultPropertyFilter = (context, propertyName) => true;

        private string _modelName;
        private ModelStateDictionary _modelState;
        private Func<ModelBindingContext, string, bool> _propertyFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingContext"/> class.
        /// </summary>
        public ModelBindingContext()
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ModelBindingContext"/> class using the
        /// <paramref name="bindingContext" />.
        /// </summary>
        /// <param name="bindingContext">Existing <see cref="ModelBindingContext"/>.</param>
        /// <param name="modelName">Model name of associated with the new <see cref="ModelBindingContext"/>.</param>
        /// <param name="modelMetadata">Model metadata of associated with the new <see cref="ModelBindingContext"/>.
        /// </param>
        public static ModelBindingContext GetChildModelBindingContext(
            [NotNull] ModelBindingContext bindingContext,
            [NotNull] string modelName,
            [NotNull] ModelMetadata modelMetadata)
        {
            var modelBindingContext = new ModelBindingContext();
            modelBindingContext.ModelName = modelName;
            modelBindingContext.ModelMetadata = modelMetadata;
            modelBindingContext.ModelState = bindingContext.ModelState;
            modelBindingContext.ValueProvider = bindingContext.ValueProvider;
            modelBindingContext.OperationBindingContext = bindingContext.OperationBindingContext;

            modelBindingContext.BindingSource = modelMetadata.BindingSource;
            modelBindingContext.BinderModelName = modelMetadata.BinderModelName;
            modelBindingContext.BinderType = modelMetadata.BinderType;
            return modelBindingContext;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="ModelBindingContext"/> from given <paramref name="metadata"/>
        /// and <paramref name="bindingInfo"/>.
        /// </summary>
        /// <param name="metadata"><see cref="ModelMetadata"/> associated with the model.</param>
        /// <param name="bindingInfo"><see cref="BindingInfo"/> associated with the model.</param>
        /// <param name="modelName">An optional name of the model to be used.</param>
        /// <returns>A new instance of <see cref="ModelBindingContext"/>.</returns>
        public static ModelBindingContext GetModelBindingContext(
            [NotNull] ModelMetadata metadata,
            [NotNull] BindingInfo bindingInfo,
            string modelName)
        {
            var binderModelName = bindingInfo.BinderModelName ?? metadata.BinderModelName;
            var propertyPredicateProvider = 
                bindingInfo.PropertyBindingPredicateProvider ?? metadata.PropertyBindingPredicateProvider;
            return new ModelBindingContext()
            {
                ModelMetadata = metadata,
                BindingSource = bindingInfo.BindingSource ?? metadata.BindingSource,
                PropertyFilter = propertyPredicateProvider?.PropertyFilter,
                BinderType = bindingInfo.BinderType ?? metadata.BinderType,
                BinderModelName = binderModelName,
                ModelName = binderModelName ?? metadata.PropertyName ?? modelName,
                FallbackToEmptyPrefix = binderModelName == null,
            };
        }

        /// <summary>
        /// Represents the <see cref="OperationBindingContext"/> associated with this context.
        /// </summary>
        public OperationBindingContext OperationBindingContext { get; set; }

        /// <summary>
        /// Gets or sets the model value for the current operation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Model"/> will typically be set for a binding operation that works
        /// against a pre-existing model object to update certain properties.
        /// </remarks>
        public object Model { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the model associated with this context.
        /// </summary>
        public ModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name of the model. This property is used as a key for looking up values in
        /// <see cref="IValueProvider"/> during model binding.
        /// </summary>
        public string ModelName
        {
            get
            {
                if (_modelName == null)
                {
                    _modelName = string.Empty;
                }
                return _modelName;
            }
            set { _modelName = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="ModelStateDictionary"/> used to capture <see cref="ModelState"/> values
        /// for properties in the object graph of the model when binding.
        /// </summary>
        public ModelStateDictionary ModelState
        {
            get
            {
                if (_modelState == null)
                {
                    _modelState = new ModelStateDictionary();
                }
                return _modelState;
            }
            set { _modelState = value; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <remarks>
        /// The <see cref="ModelMetadata"/> property must be set to access this property.
        /// </remarks>
        public Type ModelType
        {
            get
            {
                EnsureModelMetadata();
                return ModelMetadata.ModelType;
            }
        }

        /// <summary>
        /// Gets or sets a model name which is explicitly set using an <see cref="IModelNameProvider"/>. 
        /// <see cref="Model"/>.
        /// </summary>
        public string BinderModelName { get; set; }

        /// <summary>
        /// Gets or sets a value which represents the <see cref="BindingSource"/> associated with the 
        /// <see cref="Model"/>.
        /// </summary>
        public BindingSource BindingSource { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of an <see cref="IModelBinder"/> associated with the 
        /// <see cref="Model"/>.
        /// </summary>
        public Type BinderType { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the binder should use an empty prefix to look up
        /// values in <see cref="IValueProvider"/> when no values are found using the
        /// <see cref="ModelName"/> prefix.
        /// </summary>
        public bool FallbackToEmptyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueProvider"/> associated with this context.
        /// </summary>
        public IValueProvider ValueProvider { get; set; }

        public Func<ModelBindingContext, string, bool> PropertyFilter
        {
            get
            {
                if (_propertyFilter == null)
                {
                    _propertyFilter = _defaultPropertyFilter;
                }
                return _propertyFilter;
            }
            set { _propertyFilter = value; }
        }

        private void EnsureModelMetadata()
        {
            if (ModelMetadata == null)
            {
                throw new InvalidOperationException(Resources.ModelBindingContext_ModelMetadataMustBeSet);
            }
        }
    }
}
