// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    public class VersionAttribute : Attribute, IActionConstraintFactory
    {
        private int? _maxVersion;
        private int? _minVersion;

        public int MinVersion
        {
            get { return _minVersion ?? -1; }
            set { _minVersion = value; }
        }

        public int MaxVersion
        {
            get { return _maxVersion ?? -1; }
            set { _maxVersion = value; }
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return new RangeVersionValidator(_minVersion, _maxVersion);
        }
    }
}