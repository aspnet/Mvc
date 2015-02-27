// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataBindingDetails
    {
        public BindingSource BindingSource { get; set; }

        public string BinderModelName { get; set; }

        public Type BinderType { get; set; }

        public bool? IsReadOnly { get; set; }

        public bool? IsRequired { get; set; }

        public IPropertyBindingPredicateProvider PropertyBindingPredicateProvider { get; set; }
    }
}