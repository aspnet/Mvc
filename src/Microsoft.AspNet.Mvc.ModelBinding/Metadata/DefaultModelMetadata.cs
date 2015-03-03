// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DefaultModelMetadata : ModelMetadata
    {
        private readonly IModelMetadataProvider _provider;
        private readonly ICompositeMetadataDetailsProvider _detailsProvider;
        private readonly DefaultMetadataDetailsCache _cache;

        private ReadOnlyDictionary<object, object> _additionalValues;
        private bool? _isReadOnly;
        private bool? _isRequired;
        private ModelPropertyCollection _properties;

        public DefaultModelMetadata(
            [NotNull] IModelMetadataProvider provider,
            [NotNull] ICompositeMetadataDetailsProvider detailsProvider,
            [NotNull] DefaultMetadataDetailsCache cache)
            : base(cache.Key)
        {
            _provider = provider;
            _detailsProvider = detailsProvider;
            _cache = cache;
        }

        public IReadOnlyList<object> Attributes
        {
            get
            {
                return _cache.Attributes;
            }
        }

        public BindingMetadata BindingMetadata
        {
            get
            {
                if (_cache.BindingMetadata == null)
                {
                    var context = new BindingMetadataProviderContext(Identity, _cache.Attributes);
                    _detailsProvider.GetBindingMetadata(context);
                    _cache.BindingMetadata = context.BindingMetadata;
                }

                return _cache.BindingMetadata;
            }
        }

        public DisplayMetadata DisplayMetadata
        {
            get
            {
                if (_cache.DisplayMetadata == null)
                {
                    var context = new DisplayMetadataProviderContext(Identity, _cache.Attributes);
                    _detailsProvider.GetDisplayMetadata(context);
                    _cache.DisplayMetadata = context.DisplayMetadata;
                }

                return _cache.DisplayMetadata;
            }
        }

        public ValidationMetadata ValidationMetadata
        {
            get
            {
                if (_cache.ValidationMetadata == null)
                {
                    var context = new ValidationMetadataProviderContext(Identity, _cache.Attributes);
                    _detailsProvider.GetValidationMetadata(context);
                    _cache.ValidationMetadata = context.ValidationMetadata;
                }

                return _cache.ValidationMetadata;
            }
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues
        {
            get
            {
                if (_additionalValues == null)
                {
                    _additionalValues = new ReadOnlyDictionary<object, object>(DisplayMetadata.AdditionalValues);
                }

                return _additionalValues;
            }
        }

        public override BindingSource BindingSource
        {
            get
            {
                return BindingMetadata.BindingSource;
            }
        }

        public override string BinderModelName
        {
            get
            {
                return BindingMetadata.BinderModelName;
            }
        }

        public override Type BinderType
        {
            get
            {
                return BindingMetadata.BinderType;
            }
        }

        public override bool ConvertEmptyStringToNull
        {
            get
            {
                return DisplayMetadata.ConvertEmptyStringToNull;
            }
        }

        public override string DataTypeName
        {
            get
            {
                return DisplayMetadata.DataTypeName;
            }
        }

        public override string Description
        {
            get
            {
                return DisplayMetadata.Description;
            }
        }

        public override string DisplayFormatString
        {
            get
            {
                return DisplayMetadata.DisplayFormatString;
            }
        }

        public override string DisplayName
        {
            get
            {
                return DisplayMetadata.DisplayName;
            }
        }

        public override string EditFormatString
        {
            get
            {
                return DisplayMetadata.EditFormatString;
            }
        }

        public override bool HasNonDefaultEditFormat
        {
            get
            {
                return DisplayMetadata.HasNonDefaultEditFormat;
            }
        }

        public override bool HideSurroundingHtml
        {
            get
            {
                return DisplayMetadata.HideSurroundingHtml;
            }
        }

        public override bool HtmlEncode
        {
            get
            {
                return DisplayMetadata.HtmlEncode;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (!_isReadOnly.HasValue)
                {
                    if (BindingMetadata.IsReadOnly.HasValue)
                    {
                        _isReadOnly = BindingMetadata.IsReadOnly;
                    }
                    else
                    {
                        _isReadOnly = _cache.PropertySetter != null;
                    }
                }

                return _isReadOnly.Value;
            }
        }

        public override bool IsRequired
        {
            get
            {
                if (!_isRequired.HasValue)
                {
                    if (BindingMetadata.IsRequired.HasValue)
                    {
                        _isRequired = BindingMetadata.IsRequired;
                    }
                    else
                    {
                        _isRequired = !ModelType.AllowsNullValue();
                    }
                }
                
                return _isRequired.Value;
            }
        }

        public override string NullDisplayText
        {
            get
            {
                return DisplayMetadata.NullDisplayText;
            }
        }

        public override int Order
        {
            get
            {
                return DisplayMetadata.Order;
            }
        }

        public override ModelPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    var properties = _provider.GetMetadataForProperties(ModelType);
                    properties = properties.OrderBy(p => p.Order);
                    _properties = new ModelPropertyCollection(properties);
                }

                return _properties;
            }
        }

        public override IPropertyBindingPredicateProvider PropertyBindingPredicateProvider
        {
            get
            {
                return BindingMetadata.PropertyBindingPredicateProvider;
            }
        }

        public override bool ShowForDisplay
        {
            get
            {
                return DisplayMetadata.ShowForDisplay;
            }
        }

        public override bool ShowForEdit
        {
            get
            {
                return DisplayMetadata.ShowForEdit;
            }
        }

        public override string SimpleDisplayProperty
        {
            get
            {
                return DisplayMetadata.SimpleDisplayProperty;
            }
        }

        public override string TemplateHint
        {
            get
            {
                return DisplayMetadata.TemplateHint;
            }
        }
    }
}