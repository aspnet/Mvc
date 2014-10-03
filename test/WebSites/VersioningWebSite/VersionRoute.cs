﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using System.Text.RegularExpressions;

namespace VersioningWebSite
{
    public class VersionRoute : RouteAttribute, IActionConstraintFactory
    {
        private readonly IActionConstraint _constraint;

        // 5
        // [5]
        // (5)
        // (5]
        // [5)
        // (3-5)
        // (3-5]
        // [3-5)
        // [3-5]
        // [35-56]
        // Parses the above version formats and captures lb (lower bound), range, and hb (higher bound)
        private static readonly Regex _versionParser = new Regex(@"^(?<lb>[\(\[])?(?<range>\d+(-\d+)?)(?<hb>[\)\]])?$");

        public VersionRoute(string template)
            : base(template)
        {
        }

        public VersionRoute(string template, string versionRange)
            : base(template)
        {
            var constraint = CreateVersionConstraint(versionRange);

            if (constraint == null)
            {
                var message = string.Format("Invalid version format: {0}", versionRange);
                throw new ArgumentException(message, "versionRange");
            }

            _constraint = constraint;
        }

        private static IActionConstraint CreateVersionConstraint(string versionRange)
        {
            var match = _versionParser.Match(versionRange);

            if (!match.Success)
            {
                return null;
            }

            var lowerBound = match.Groups["lb"].Value;
            var higherBound = match.Groups["hb"].Value;
            var range = match.Groups["range"].Value;

            var rangeValues = range.Split('-');
            if (rangeValues.Length == 1)
            {
                return GetSingleVersionOrHigherUnboundedVersionConstraint(lowerBound, higherBound, rangeValues);
            }
            else
            {
                return GetBoundedRangeVersionConstraint(lowerBound, higherBound, rangeValues);
            }
        }

        private static IActionConstraint GetBoundedRangeVersionConstraint(
            string lowerBound,
            string higherBound,
            string[] rangeValues)
        {
            // [3-5, (3-5, 3-5], 3-5), 3-5 are not valid
            if (string.IsNullOrEmpty(lowerBound) || string.IsNullOrEmpty(higherBound))
            {
                return null;
            }

            var minVersion = int.Parse(rangeValues[0]);
            var maxVersion = int.Parse(rangeValues[1]);

            // Adjust min version and max version if the limit is exclusive.
            minVersion = lowerBound == "(" ? minVersion + 1 : minVersion;
            maxVersion = higherBound == ")" ? maxVersion - 1 : maxVersion;

            if (minVersion > maxVersion)
            {
                return null;
            }

            return new VersionRangeValidator(minVersion, maxVersion);
        }

        private static IActionConstraint GetSingleVersionOrHigherUnboundedVersionConstraint(
            string lowerBound,
            string higherBound,
            string[] rangeValues)
        {
            // (5], [5), (5), [5, (5, 5], 5) are not valid
            if (lowerBound == "(" || higherBound == ")" ||
                (string.IsNullOrEmpty(lowerBound) ^ string.IsNullOrEmpty(higherBound)))
            {
                return null;
            }

            var version = int.Parse(rangeValues[0]);
            if (!string.IsNullOrEmpty(lowerBound))
            {
                // [5]
                return new VersionRangeValidator(version, version);
            }
            else
            {
                // 5
                return new VersionRangeValidator(version, maxVersion: null);
            }
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return _constraint;
        }
    }
}