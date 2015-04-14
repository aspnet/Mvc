// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;

namespace InlineConstraintsWebSite.Constraints
{
    public class IsbnDigitScheme10Constraint : IRouteConstraint
    {
        private readonly bool _allowDashes;

        public IsbnDigitScheme10Constraint(bool allowDashes)
        {
            _allowDashes = allowDashes;
        }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            IDictionary<string, object> values,
            RouteDirection routeDirection)
        {
            var isbnRegExpression = @"^(\d{9})(\d|X)\z";

            if (_allowDashes)
            {
                isbnRegExpression = @"^(\d{1})[-]\d{3}[-]\d{5}[-][\d|X]\z";
            }

            object value;

            if (!values.TryGetValue(routeKey, out value))
            {
                return false;
            }

            var isbnNumber = value as string;
            var isbnRegEx = new Regex(isbnRegExpression, RegexOptions.IgnoreCase);

            if (isbnNumber == null
                || !isbnRegEx.Match(isbnNumber).Success)
            {
                return false;
            }

            var sum = 0;
            Func<char, int> convertToInt = (char n) => (int)n - (int)'0';

            if (_allowDashes)
            {
                isbnNumber = isbnNumber = isbnNumber.Replace("-", string.Empty);
            }

            for (int i = 0; i < isbnNumber.Length - 1; ++i)
            {
                sum += convertToInt(isbnNumber[i]) * (i + 1);
            }

            var checkSum = sum % 11;
            var lastDigit = isbnNumber.Last();

            if (checkSum == 10)
            {
                return char.ToUpperInvariant(lastDigit) == 'X';
            }
            else
            {
                return checkSum == convertToInt(lastDigit);
            }
        }
    }
}