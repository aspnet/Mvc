// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
#if NET45
using System.ComponentModel;
#endif
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class CachedDataAnnotationsMetadataAttributes
    {
        public CachedDataAnnotationsMetadataAttributes(IEnumerable<Attribute> attributes)
        {
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            DisplayColumn = attributes.OfType<DisplayColumnAttribute>().FirstOrDefault();
#if NET45
            DisplayName = attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
#endif
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            Required = attributes.OfType<RequiredAttribute>().FirstOrDefault();
            ScaffoldColumn = attributes.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
        }

        public DisplayAttribute Display { get; protected set; }

        public DisplayFormatAttribute DisplayFormat { get; protected set; }

        public DisplayColumnAttribute DisplayColumn { get; protected set; }

#if NET45
        public DisplayNameAttribute DisplayName { get; protected set; }
#endif

        public EditableAttribute Editable { get; protected set; }

        public RequiredAttribute Required { get; protected set; }

        public ScaffoldColumnAttribute ScaffoldColumn { get; protected set; }
    }
}
