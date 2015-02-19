// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Associates a model object with it's corresponding <see cref="ModelMetadata"/>.
    /// </summary>
    public class ModelExplorer
    {
        private object _model;
        private Func<object> _modelAccessor;
        private Type _modelType;

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="model">The model object. May be <c>null</c>.</param>
        public ModelExplorer([NotNull] ModelMetadata metadata, object model)
        {
            Metadata = metadata;
            Model = model;
        }

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="modelAccessor">An accessor for the model object. May be <c>null</c>.</param>
        public ModelExplorer([NotNull] ModelMetadata metadata, Func<object> modelAccessor)
        {
            Metadata = metadata;
            _modelAccessor = modelAccessor;
        }

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="model">An accessor for the model object. May be <c>null</c>.</param>
        /// <param name="container">A container <see cref="ModelExplorer"/>. May be <c>null</c>.</param>
        public ModelExplorer([NotNull] ModelMetadata metadata, object model, ModelExplorer container)
        {
            Metadata = metadata;
            Model = model;
            Container = container;
        }

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="modelAccessor">An accessor for the model object. May be <c>null</c>.</param>
        /// <param name="container">A container <see cref="ModelExplorer"/>. May be <c>null</c>.</param>
        public ModelExplorer([NotNull] ModelMetadata metadata, Func<object> modelAccessor, ModelExplorer container)
        {
            Metadata = metadata;
            _modelAccessor = modelAccessor;
            Container = container;
        }

        /// <summary>
        /// Gets the container <see cref="ModelExplorer"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Container"/> will most commonly be set as a result of calling
        /// <see cref="GetProperty(string)"/>. In this case, the returned <see cref="ModelExplorer"/> will
        /// have it's <see cref="Container"/> set to the instance upon which <see cref="GetProperty(string)"/>
        /// was called.
        /// 
        /// This however is not a requirement. The <see cref="Container"/> is informational, and may not
        /// represent a type that defines the property represented by <see cref="Metadata"/>. This can
        /// occur when constructing a <see cref="ModelExplorer"/> based on evaluation of a complex
        /// expression.
        /// 
        /// If calling code relies on a parent-child relationship between <see cref="ModelExplorer"/>
        /// instances, then use <see cref="ModelMetadata.ContainerType"/> to validate this assumption.
        /// </remarks>
        public ModelExplorer Container { get; }

        /// <summary>
        /// Gets the <see cref="ModelMetadata"/>.
        /// </summary>
        public ModelMetadata Metadata { get; }

        /// <summary>
        /// Gets the model object.
        /// </summary>
        /// <remarks>
        /// Retrieving the <see cref="Model"/> object will for execution of the model accessor function if this
        /// <see cref="ModelExplorer"/> was provided with one.
        /// </remarks>
        public object Model
        {
            get
            {
                if (_model == null && _modelAccessor != null)
                {
                    _model = _modelAccessor();

                    // Null-out the accessor so we don't invoke it repeatedly if it returns null.
                    _modelAccessor = null;
                }

                return _model;
            }

            private set
            {
                Debug.Assert(_modelAccessor == null);
                _model = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the property, or <c>null</c> if the property cannot be
        /// found.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>A <see cref="ModelExplorer"/>, or <c>null</c>.</returns>
        public ModelExplorer GetProperty(string name)
        {
            var propertyMetadata = Metadata.Properties[name];
            if (propertyMetadata == null)
            {
                return null;
            }

            if (Model == null)
            {
                return new ModelExplorer(propertyMetadata, model: null, container: this);
            }
            else
            {
                var propertyHelper = PropertyHelper.GetProperties(Model.GetType()).Where(p => p.Name == name).FirstOrDefault();
                if (propertyHelper == null)
                {
                    return new ModelExplorer(propertyMetadata, model: null, container: this);
                }

                var accessor = PropertyHelper.MakeFastPropertyGetter(propertyHelper.Property);
                return new ModelExplorer(propertyMetadata, model: accessor(Model), container: this);
            }
        }

        /// <remarks>
        /// Retrieving the <see cref="Model"/> type will for execution of the model accessor function if this
        /// <see cref="ModelExplorer"/> was provided with one.
        /// </remarks>
        public Type ModelType
        {
            get
            {
                if (_modelType == null)
                {
                    if (Model == null)
                    {
                        // If the model is null, then use the declared model type;
                        _modelType = Metadata.ModelType;
                    }
                    else if (Metadata.IsNullableValueType)
                    {
                        // We have a model, but if it's a nullable value type, then Model.GetType() will return
                        // the non-nullable type (int? -> int). Since it's a value type, there's no subclassing,
                        // just go with the declared type.
                        _modelType = Metadata.ModelType;
                    }
                    else
                    {
                        // We have a model, and it's not a nullable so use the runtime type to handle
                        // cases where the model is a subclass of the declared type and has extra data.
                        _modelType = Model.GetType();
                    }
                }

                return _modelType;
            }
        }
    }
}