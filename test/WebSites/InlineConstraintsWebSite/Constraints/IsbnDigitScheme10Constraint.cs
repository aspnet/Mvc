// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;

namespace InlineConstraintsWebSite.Constraints
{
    public class IsbnDigitScheme10Constraint : IRouteConstraint
    {
        private readonly bool _turnOn;

        public IsbnDigitScheme10Constraint(bool turnOn)
        {
            _turnOn = turnOn;
        }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            IDictionary<string, object> values,
            RouteDirection routeDirection)
        {
            if (!_turnOn)
            {
                return true;
            }

            object value;
            var isbnNumbers = string.Empty;

            if (values.TryGetValue(routeKey, out value))
            {
                isbnNumbers = value as string;

                if (isbnNumbers.Any(n => !char.IsDigit(n)))
                {
                    return false;
                }

                if (isbnNumbers.Length != 10)
                {
                    return false;
                }
            }

            var sum = 0;
            for (int i = 0; i < isbnNumbers.Length - 1; ++i)
            {
                sum += ConvertToInt(isbnNumbers[i]) * (i + 1);
            }

            var checkSum = sum % 11;
            if (checkSum == ConvertToInt(isbnNumbers.Last()))
            {
                return true;
            }

            return false;
        }

        private int ConvertToInt(char n)
        {
            return (int)char.GetNumericValue(n);
        }
    }
}