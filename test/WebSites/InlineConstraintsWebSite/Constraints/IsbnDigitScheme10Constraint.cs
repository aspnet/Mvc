// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

            if (!values.TryGetValue(routeKey, out value))
            {
                return false;
            }

            var isbnNumber = value as string;

            if (isbnNumber == null
                || isbnNumber.Length != 10
                || isbnNumber.Any(n => !char.IsDigit(n)))
            {
                return false;
            }

            var sum = 0;
            Func<char, int> convertToInt = (char n) => (int)char.GetNumericValue(n);

            for (int i = 0; i < isbnNumber.Length - 1; ++i)
            {
                sum += convertToInt(isbnNumber[i]) * (i + 1);
            }

            var checkSum = sum % 11;

            if (checkSum == convertToInt(isbnNumber.Last()))
            {
                return true;
            }

            return false;
        }
    }
}