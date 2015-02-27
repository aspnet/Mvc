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
        private readonly ICompositeModelMetadataDetailsProvider _detailsProvider;
        private readonly ModelMetadataDetailsCache _cache;

        private ReadOnlyDictionary<object, object> _additionalValues;
        private bool? _isReadOnly;
        private bool? _isRequired;
        private ModelPropertyCollection _properties;

        public DefaultModelMetadata(
            [NotNull] IModelMetadataProvider provider,
            [NotNull] ICompositeModelMetadataDetailsProvider detailsProvider,
            [NotNull] ModelMetadataDetailsCache cache)
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

        public ModelMetadataBindingDetails BindingDetails
        {
            get
            {
                if (_cache.BindingDetails == null)
                {
                    var context = new ModelMetadataBindingDetailsContext(Identity, _cache.Attributes);
                    _detailsProvider.GetBindingDetails(context);
                    _cache.BindingDetails = context.BindingDetails;
                }

                return _cache.BindingDetails;
            }
        }

        public ModelMetadataDisplayDetails DisplayDetails
        {
            get
            {
                if (_cache.DisplayDetails == null)
                {
                    var context = new ModelMetadataDisplayDetailsContext(Identity, _cache.Attributes);
                    _detailsProvider.GetDisplayDetails(context);
                    _cache.DisplayDetails = context.DisplayDetails;
                }

                return _cache.DisplayDetails;
            }
        }

        public ModelMetadataValidationDetails ValidationDetails
        {
            get
            {
                if (_cache.ValidationDetails == null)
                {
                    var context = new ModelMetadataValidationDetailsContext(Identity, _cache.Attributes);
                    _detailsProvider.GetValidationDetails(context);
                    _cache.ValidationDetails = context.ValidationDetails;
                }

                return _cache.ValidationDetails;
            }
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues
        {
            get
            {
                if (_additionalValues == null)
                {
                    _additionalValues = new ReadOnlyDictionary<object, object>(DisplayDetails.AdditionalValues);
                }

                return _additionalValues;
            }
        }

        public override BindingSource BindingSource
        {
            get
            {
                return BindingDetails.BindingSource;
            }
        }

        public override string BinderModelName
        {
            get
            {
                return BindingDetails.BinderModelName;
            }
        }

        public override Type BinderType
        {
            get
            {
                return BindingDetails.BinderType;
            }
        }

        public override bool ConvertEmptyStringToNull
        {
            get
            {
                return DisplayDetails.ConvertEmptyStringToNull;
            }
        }

        public override string DataTypeName
        {
            get
            {
                return DisplayDetails.DataTypeName;
            }
        }

        public override string Description
        {
            get
            {
                return DisplayDetails.Description;
            }
        }

        public override string DisplayFormatString
        {
            get
            {
                return DisplayDetails.DisplayFormatString;
            }
        }

        public override string DisplayName
        {
            get
            {
                return DisplayDetails.DisplayName;
            }
        }

        public override string EditFormatString
        {
            get
            {
                return DisplayDetails.EditFormatString;
            }
        }

        public override bool HasNonDefaultEditFormat
        {
            get
            {
                return DisplayDetails.HasNonDefaultEditFormat;
            }
        }

        public override bool HideSurroundingHtml
        {
            get
            {
                return DisplayDetails.HideSurroundingHtml;
            }
        }

        public override bool HtmlEncode
        {
            get
            {
                return DisplayDetails.HtmlEncode;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (!_isReadOnly.HasValue)
                {
                    if (BindingDetails.IsReadOnly.HasValue)
                    {
                        _isReadOnly = BindingDetails.IsReadOnly;
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
                    if (BindingDetails.IsRequired.HasValue)
                    {
                        _isRequired = BindingDetails.IsRequired;
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
                return DisplayDetails.NullDisplayText;
            }
        }

        public override int Order
        {
            get
            {
                return DisplayDetails.Order;
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
                return BindingDetails.PropertyBindingPredicateProvider;
            }
        }

        public override bool ShowForDisplay
        {
            get
            {
                return DisplayDetails.ShowForDisplay;
            }
        }

        public override bool ShowForEdit
        {
            get
            {
                return DisplayDetails.ShowForEdit;
            }
        }

        public override string SimpleDisplayProperty
        {
            get
            {
                return DisplayDetails.SimpleDisplayProperty;
            }
        }

        public override string TemplateHint
        {
            get
            {
                return DisplayDetails.TemplateHint;
            }
        }
    }
}