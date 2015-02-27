// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class ModelMetadata
    {
        public static readonly int DefaultOrder = 10000;

        protected ModelMetadata([NotNull] ModelMetadataIdentity identity)
        {
            Identity = identity;
        }

        #region Owned Properties

        public Type ContainerType { get { return Identity.ContainerType; } }

        public ModelMetadataKind MetadataKind { get { return Identity.MetadataKind; } }

        public Type ModelType { get { return Identity.ModelType; } }

        public string PropertyName
        {
            get
            {
                return Identity.Name;
            }
        }

        protected ModelMetadataIdentity Identity { get; }

        #endregion

        #region Abstract Properties

        /// <summary>
        /// Gets a collection of additional information about the model.
        /// </summary>
        public abstract IReadOnlyDictionary<object, object> AdditionalValues { get; }

        /// <summary>
        /// Gets the collection of <see cref="ModelMetadata"/> instances for the model's properties.
        /// </summary>
        public abstract ModelPropertyCollection Properties { get; }

        /// <summary>
        /// Gets the name of a model if specified explicitly using <see cref="IModelNameProvider"/>.
        /// </summary>
        public abstract string BinderModelName { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of an <see cref="IModelBinder"/> or an
        /// <see cref="IModelBinderProvider"/> of a model if specified explicitly using 
        /// <see cref="IBinderTypeProviderMetadata"/>.
        /// </summary>
        public abstract Type BinderType { get; }

        /// <summary>
        /// Gets a binder metadata for this model.
        /// </summary>
        public abstract ModelBinding.BindingSource BindingSource { get; }

        public abstract bool ConvertEmptyStringToNull { get; }

        /// <summary>
        /// Gets the name of the <see cref="Model"/>'s datatype.  Overrides <see cref="ModelType"/> in some
        /// display scenarios.
        /// </summary>
        /// <value><c>null</c> unless set manually or through additional metadata e.g. attributes.</value>
        public abstract string DataTypeName { get; }

        public abstract string Description { get; }

        /// <summary>
        /// Gets the composite format <see cref="string"/> (see
        /// http://msdn.microsoft.com/en-us/library/txafckwd.aspx) used to display the <see cref="Model"/>.
        /// </summary>
        public abstract string DisplayFormatString { get; }

        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets the composite format <see cref="string"/> (see
        /// http://msdn.microsoft.com/en-us/library/txafckwd.aspx) used to edit the <see cref="Model"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IModelMetadataProvider"/> instances that set this property to a non-<c>null</c>, non-empty,
        /// non-default value should also set <see cref="HasNonDefaultEditFormat"/> to <c>true</c>.
        /// </remarks>
        public abstract string EditFormatString { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="EditFormatString"/> has a non-<c>null</c>, non-empty
        /// value different from the default for the datatype.
        /// </summary>
        public abstract bool HasNonDefaultEditFormat { get; }

        /// <summary>
        /// Gets a value indicating whether the value should be HTML-encoded.
        /// </summary>
        /// <value>If <c>true</c>, value should be HTML-encoded. Default is <c>true</c>.</value>
        public abstract bool HtmlEncode { get; }

        /// <summary>
        /// Gets a value indicating whether the "HiddenInput" display template should return
        /// <c>string.Empty</c> (not the expression value) and whether the "HiddenInput" editor template should not
        /// also return the expression value (together with the hidden &lt;input&gt; element).
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, also causes the default <see cref="object"/> display and editor templates to return HTML
        /// lacking the usual per-property &lt;div&gt; wrapper around the associated property. Thus the default
        /// <see cref="object"/> display template effectively skips the property and the default <see cref="object"/>
        /// editor template returns only the hidden &lt;input&gt; element for the property.
        /// </remarks>
        public abstract bool HideSurroundingHtml { get; }

        public abstract bool IsReadOnly { get; }

        public abstract bool IsRequired { get; }

        /// <summary>
        /// Gets a value indicating where the current metadata should be ordered relative to other properties
        /// in its containing type.
        /// </summary>
        /// <remarks>
        /// <para>For example this property is used to order items in <see cref="Properties"/>.</para>
        /// <para>The default order is <c>10000</c>.</para>
        /// </remarks>
        /// <value>The order value of the current metadata.</value>
        public abstract int Order { get; }

        public abstract string NullDisplayText { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyBindingPredicateProvider"/>, which can determine which properties
        /// should be model bound.
        /// </summary>
        public abstract ModelBinding.IPropertyBindingPredicateProvider PropertyBindingPredicateProvider { get; }

        /// <summary>
        /// Gets a value that indicates whether the property should be displayed in read-only views.
        /// </summary>
        public abstract bool ShowForDisplay { get; }

        /// <summary>
        /// Gets a value that indicates whether the property should be displayed in editable views.
        /// </summary>
        public abstract bool ShowForEdit { get; }

        /// <summary>
        /// Gets  a value which is the name of the property used to display the model.
        /// </summary>
        public abstract string SimpleDisplayProperty { get; }

        public abstract string TemplateHint { get; }

        #endregion

        #region Computed Properties

        public virtual bool IsComplexType
        {
            get { return !TypeHelper.HasStringConverter(ModelType); }
        }

        public bool IsNullableValueType
        {
            get { return ModelType.IsNullableValueType(); }
        }

        public virtual bool IsCollectionType
        {
            get { return TypeHelper.IsCollectionType(ModelType); }
        }

        #endregion

        public string GetDisplayName()
        {
            return DisplayName ?? PropertyName ?? ModelType.Name;
        }
    }
} 
