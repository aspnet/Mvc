﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Associates a model object with it's corresponding <see cref="ModelMetadata"/>.
    /// </summary>
    [DebuggerDisplay("DeclaredType={Metadata.ModelType.Name} PropertyName={Metadata.PropertyName}")]
    public class ModelExplorer
    {
        private readonly IModelMetadataProvider _metadataProvider;

        private object _model;
        private Func<object, object> _modelAccessor;
        private Type _modelType;
        private List<ModelExplorer> _properties;

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="model">The model object. May be <c>null</c>.</param>
        public ModelExplorer(
            [NotNull] IModelMetadataProvider metadataProvider, 
            [NotNull] ModelMetadata metadata, 
            object model)
        {
            _metadataProvider = metadataProvider;
            Metadata = metadata;
            _model = model;
        }

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="container">The container <see cref="ModelExplorer"/>.</param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="modelAccessor">A model accessor function. May be <c>null</c>.</param>
        public ModelExplorer(
            [NotNull] IModelMetadataProvider metadataProvider,
            [NotNull] ModelExplorer container,
            [NotNull] ModelMetadata metadata,
            Func<object, object> modelAccessor)
        {
            _metadataProvider = metadataProvider;
            Container = container;
            Metadata = metadata;
            _modelAccessor = modelAccessor;
        }

        /// <summary>
        /// Creates a new <see cref="ModelExplorer"/>.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="container">The container <see cref="ModelExplorer"/>.</param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        /// <param name="model">The model object. May be <c>null</c>.</param>
        public ModelExplorer(
            [NotNull] IModelMetadataProvider metadataProvider,
            [NotNull] ModelExplorer container,
            [NotNull] ModelMetadata metadata,
            object model)
        {
            _metadataProvider = metadataProvider;
            Container = container;
            Metadata = metadata;
            _model = model;
        }

        /// <summary>
        /// Gets the container <see cref="ModelExplorer"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="Container"/> will most commonly be set as a result of calling
        /// <see cref="GetExplorerForProperty(string)"/>. In this case, the returned <see cref="ModelExplorer"/> will
        /// have it's <see cref="Container"/> set to the instance upon which <see cref="GetExplorerForProperty(string)"/>
        /// was called.
        /// </para>
        /// <para>
        /// This however is not a requirement. The <see cref="Container"/> is informational, and may not
        /// represent a type that defines the property represented by <see cref="Metadata"/>. This can
        /// occur when constructing a <see cref="ModelExplorer"/> based on evaluation of a complex
        /// expression.
        /// </para>
        /// <para>
        /// If calling code relies on a parent-child relationship between <see cref="ModelExplorer"/>
        /// instances, then use <see cref="ModelMetadata.ContainerType"/> to validate this assumption.
        /// </para>
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
        /// Retrieving the <see cref="Model"/> object will execute the model accessor function if this
        /// <see cref="ModelExplorer"/> was provided with one.
        /// </remarks>
        public object Model
        {
            get
            {
                if (_model == null && _modelAccessor != null)
                {
                    Debug.Assert(Container != null);
                    _model = _modelAccessor(Container.Model);

                    // Null-out the accessor so we don't invoke it repeatedly if it returns null.
                    _modelAccessor = null;
                }

                return _model;
            }
        }

        /// <remarks>
        /// Retrieving the <see cref="ModelType"/> will execute the model accessor function if this
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

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <remarks>
        /// Includes a <see cref="ModelExplorer"/> for each property of the <see cref="ModelMetadata"/>
        /// for <see cref="ModelType"/>. 
        /// </remarks>
        public IEnumerable<ModelExplorer> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new List<ModelExplorer>();

                    var metadata = GetMetadataForRuntimeType();

                    var properties = Enumerable.Join(
                        metadata.Properties,
                        PropertyHelper.GetProperties(ModelType),
                        m => m.PropertyName,
                        ph => ph.Property.Name,
                        (m, ph) => CreateExplorerForProperty(m, ph));

                    _properties.AddRange(properties);
                }

                return _properties;
            }
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the given <paramref name="model"/> value.
        /// </summary>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>.</returns>
        public ModelExplorer GetExplorerForModel(object model)
        {
            if (Container == null)
            {
                return new ModelExplorer(_metadataProvider, Metadata, model);
            }
            else
            {
                return new ModelExplorer(_metadataProvider, Container, Metadata, model);
            }
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the property with given <paramref name="name"/>, or <c>null</c> if
        /// the property cannot be found.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>A <see cref="ModelExplorer"/>, or <c>null</c>.</returns>
        public ModelExplorer GetExplorerForProperty([NotNull] string name)
        {
            return Properties.FirstOrDefault(p => string.Equals(
                p.Metadata.PropertyName, 
                name, 
                StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the property with given <paramref name="name"/>, or <c>null</c> if
        /// the property cannot be found.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="modelAccessor">An accessor for the model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>, or <c>null</c>.</returns>
        /// <remarks>
        /// As this creates a model explorer with a specific model accessor function, the result is not cached.
        /// </remarks>
        public ModelExplorer GetExplorerForProperty([NotNull] string name, Func<object, object> modelAccessor)
        {
            var metadata = GetMetadataForRuntimeType();

            var propertyMetadata = metadata.Properties[name];
            if (propertyMetadata == null)
            {
                return null;
            }

            return new ModelExplorer(_metadataProvider, this, propertyMetadata, modelAccessor);
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the property with given <paramref name="name"/>, or <c>null</c> if
        /// the property cannot be found.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>, or <c>null</c>.</returns>
        /// <remarks>
        /// As this creates a model explorer with a specific model value, the result is not cached.
        /// </remarks>
        public ModelExplorer GetExplorerForProperty([NotNull] string name, object model)
        {
            var metadata = GetMetadataForRuntimeType();

            var propertyMetadata = metadata.Properties[name];
            if (propertyMetadata == null)
            {
                return null;
            }

            return new ModelExplorer(_metadataProvider, this, propertyMetadata, model);
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the provided model value and model <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The model <see cref="Type"/>.</param>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>.</returns>
        /// <remarks>
        /// <para>
        /// A <see cref="ModelExplorer"/> created by <see cref="GetExplorerForExpression(Type, object)"/>
        /// represents the result of executing an arbitrary expression against the model contained
        /// in the current <see cref="ModelExplorer"/> instance.
        /// </para>
        /// <para>
        /// The returned <see cref="ModelExplorer"/> will have the current instance set as its <see cref="Container"/>.
        /// </para>
        /// </remarks>
        public ModelExplorer GetExplorerForExpression([NotNull] Type modelType, object model)
        {
            var metadata = _metadataProvider.GetMetadataForType(modelType);
            return GetExplorerForExpression(metadata, model);
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the provided model value and model <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The model <see cref="Type"/>.</param>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>.</returns>
        /// <remarks>
        /// <para>
        /// A <see cref="ModelExplorer"/> created by
        /// <see cref="GetExplorerForExpression(ModelMetadata, object)"/>
        /// represents the result of executing an arbitrary expression against the model contained
        /// in the current <see cref="ModelExplorer"/> instance.
        /// </para>
        /// <para>
        /// The returned <see cref="ModelExplorer"/> will have the current instance set as its <see cref="Container"/>.
        /// </para>
        /// </remarks>
        public ModelExplorer GetExplorerForExpression([NotNull] ModelMetadata metadata, object model)
        {
            return new ModelExplorer(_metadataProvider, this, metadata, model);
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the provided model value and model <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The model <see cref="Type"/>.</param>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>.</returns>
        /// <remarks>
        /// <para>
        /// A <see cref="ModelExplorer"/> created by 
        /// <see cref="GetExplorerForExpression(Type, Func{object, object})"/>
        /// represents the result of executing an arbitrary expression against the model contained
        /// in the current <see cref="ModelExplorer"/> instance.
        /// </para>
        /// <para>
        /// The returned <see cref="ModelExplorer"/> will have the current instance set as its <see cref="Container"/>.
        /// </para>
        /// </remarks>
        public ModelExplorer GetExplorerForExpression([NotNull] Type modelType, Func<object, object> modelAccessor)
        {
            var metadata = _metadataProvider.GetMetadataForType(modelType);
            return GetExplorerForExpression(metadata, modelAccessor);
        }

        /// <summary>
        /// Gets a <see cref="ModelExplorer"/> for the provided model value and model <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The model <see cref="Type"/>.</param>
        /// <param name="model">The model value.</param>
        /// <returns>A <see cref="ModelExplorer"/>.</returns>
        /// <remarks>
        /// <para>
        /// A <see cref="ModelExplorer"/> created by 
        /// <see cref="GetExplorerForExpression(ModelMetadata, Func{object, object})"/>
        /// represents the result of executing an arbitrary expression against the model contained
        /// in the current <see cref="ModelExplorer"/> instance.
        /// </para>
        /// <para>
        /// The returned <see cref="ModelExplorer"/> will have the current instance set as its <see cref="Container"/>.
        /// </para>
        /// </remarks>
        public ModelExplorer GetExplorerForExpression([NotNull] ModelMetadata metadata, Func<object, object> modelAccessor)
        {
            return new ModelExplorer(_metadataProvider, this, metadata, modelAccessor);
        }

        private ModelMetadata GetMetadataForRuntimeType()
        {
            // We want to make sure we're looking at the runtime properties of the model, and for
            // that we need the model metadata of the runtime type.
            var metadata = Metadata;
            if (Metadata.ModelType != ModelType)
            {
                metadata = _metadataProvider.GetMetadataForType(ModelType);
            }

            return metadata;
        }

        private ModelExplorer CreateExplorerForProperty(
            ModelMetadata propertyMetadata, 
            PropertyHelper propertyHelper)
        {
            if (propertyHelper == null)
            {
                return new ModelExplorer(_metadataProvider, this, propertyMetadata, modelAccessor: null);
            }

            var modelAccessor = new Func<object, object>((c) =>
            {
                return c == null ? null : propertyHelper.GetValue(c);
            });

            return new ModelExplorer(_metadataProvider, this, propertyMetadata, modelAccessor);
        }
    }
}