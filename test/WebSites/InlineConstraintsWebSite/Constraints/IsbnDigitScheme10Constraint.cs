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
        private readonly static Regex _isbnRegex
            = new Regex(@"\A(\d{9})(\d|X)\z", RegexOptions.IgnoreCase);

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
            object value;

            if (!values.TryGetValue(routeKey, out value))
            {
                return false;
            }

            var inputString = value as string;
            string isbnNumber;

            if (inputString == null
                || !TryGetIsbn10(inputString, _allowDashes, out isbnNumber))
            {
                return false;
            }

            var sum = 0;
            Func<char, int> convertToInt = (char n) => n - '0';

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

        private static bool TryGetIsbn10(string value, bool allowDashes, out string isbnNumber)
        {
            if (!allowDashes)
            {
                if (_isbnRegex.IsMatch(value))
                {
                    isbnNumber = value;
                    return true;
                }
                else
                {
                    isbnNumber = null;
                    return false;
                }
            }

            var isbnParts = value.Split(
                new char[] { '-' },
                StringSplitOptions.RemoveEmptyEntries);

            if (isbnParts.Length == 4)
            {
                value = value.Replace("-", string.Empty);
                if (_isbnRegex.IsMatch(value))
                {
                    isbnNumber = value;
                    return true;
                }
            }

            isbnNumber = null;
            return false;
        }
    }
}