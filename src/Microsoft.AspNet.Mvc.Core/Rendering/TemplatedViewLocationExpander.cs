// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// A <see cref="IViewLocationExpander"/> that replaces tokens of the format <c>{key}</c> in view location strings 
    /// with values from the value dictionary as part of 
    /// <see cref="ExpandViewLocations(IImmutableList{string}, IDictionary{string, string})"/>.
    /// </summary>
    public class TemplatedViewLocationExpander : IViewLocationExpander
    {
        private static readonly Task _completed = Task.FromResult(0);
        private static readonly Regex _templateRegex = new Regex(@"(?<!\{){([^\}]+)\}(?!\})",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        /// <inheritdoc />
        public Task PopulateValuesAsync([NotNull] ActionContext actionContext,
                                        [NotNull] IDictionary<string, string> values)
        {
            // Do nothing.
            return _completed;
        }

        /// <inheritdoc />
        public IImmutableList<string> ExpandViewLocations([NotNull] IImmutableList<string> viewLocations, 
                                                          [NotNull] IDictionary<string, string> values)
        {
            var transformed = viewLocations.Select(location => TransformString(location, values));
            return ImmutableList.CreateRange(transformed);
        }

        private string TransformString(string location, IDictionary<string, string> values)
        {
            return _templateRegex.Replace(location, match => Replace(match, values));
        }

        private static string Replace(Match match, IDictionary<string, string> values)
        {
            Debug.Assert(match.Success, "Matches in Replace are guaranteed to be successful.");
            
            // A match has a token '{1-or-more-characters}'. Remove the curly braces.
            var key = match.Value.Substring(1, match.Value.Length - 2);
            if (values.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new InvalidOperationException(Resources.FormatTemplatedViewLocationExpander_NoReplacementToken(key));
        }
    }
}