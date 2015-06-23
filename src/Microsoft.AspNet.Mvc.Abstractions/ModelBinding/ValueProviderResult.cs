// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Result of an <see cref="IValueProvider.GetValueAsync"/> operation.
    /// </summary>
    public class ValueProviderResult
    {
        private static readonly CultureInfo _staticCulture = CultureInfo.InvariantCulture;
        private CultureInfo _instanceCulture;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ValueProviderResult"/> class. Subclass must at least set
        /// <see cref="Culture"/> to avoid <see cref="Exception"/>s in <see cref="ConvertTo(Type, CultureInfo)"/>.
        /// </summary>
        protected ValueProviderResult()
        {
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="ValueProviderResult"/> class with given
        /// <paramref name="rawValue"/>. Initializes <see cref="Culture"/> to
        /// <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="rawValue">The <see cref="RawValue"/> value of the new instance.</param>
        public ValueProviderResult(object rawValue)
            : this(rawValue, attemptedValue: null, culture: _staticCulture)
        {
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="ValueProviderResult"/> class with given
        /// <paramref name="rawValue"/>, <paramref name="attemptedValue"/>, and <paramref name="culture"/>.
        /// </summary>
        /// <param name="rawValue">The <see cref="RawValue"/> value of the new instance.</param>
        /// <param name="attemptedValue">The <see cref="AttemptedValue"/> value of the new instance.</param>
        /// <param name="culture">The <see cref="Culture"/> value of the new instance.</param>
        public ValueProviderResult(object rawValue, string attemptedValue, CultureInfo culture)
        {
            RawValue = rawValue;
            AttemptedValue = attemptedValue;
            Culture = culture;
        }

        /// <summary>
        /// <see cref="string"/> conversion of <see cref="RawValue"/>.
        /// </summary>
        /// <remarks>
        /// Used in helpers that generate <c>&lt;textarea&gt;</c> elements as well as some error messages.
        /// </remarks>
        public string AttemptedValue { get; protected set; }

        /// <summary>
        /// <see cref="CultureInfo"/> to use in <see cref="ConvertTo(Type)"/> or
        /// <see cref="ConvertTo(Type, CultureInfo)"/> if passed <see cref="CultureInfo"/> is <c>null</c>.
        /// </summary>
        public CultureInfo Culture
        {
            get
            {
                if (_instanceCulture == null)
                {
                    _instanceCulture = _staticCulture;
                }
                return _instanceCulture;
            }
            protected set { _instanceCulture = value; }
        }

        /// <summary>
        /// The provided <see cref="object"/>.
        /// </summary>
        public object RawValue { get; protected set; }

        /// <summary>
        /// Converts <see cref="RawValue"/> to the given <paramref name="type"/>. Uses <see cref="Culture"/> for
        /// <see cref="TypeConverter"/> operations.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/> of the conversion.</param>
        /// <returns>
        /// <see cref="RawValue"/> converted to the given <paramref name="type"/>. <c>null</c> if the conversion fails.
        /// </returns>
        public object ConvertTo(Type type)
        {
            return ConvertTo(type, culture: null);
        }

        /// <summary>
        /// Converts <see cref="RawValue"/> to the given <paramref name="type"/> using the given
        /// <paramref name="culture"/>.
        /// </summary>
        /// <param name="type">The target <see cref="Type"/> of the conversion.</param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use for <see cref="TypeConverter"/> operations. Uses
        /// <see cref="Culture"/> if this parameter is <c>null</c>.
        /// </param>
        /// <returns>
        /// <see cref="RawValue"/> converted to the given <paramref name="type"/> using the given
        /// <paramref name="culture"/>. <c>null</c> if the conversion fails.
        /// </returns>
        public virtual object ConvertTo([NotNull] Type type, CultureInfo culture)
        {
            var value = RawValue;
            if (value == null)
            {
                // treat null route parameters as though they were the default value for the type
                return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) :
                                                        null;
            }

            if (value.GetType().IsAssignableFrom(type))
            {
                return value;
            }

            var cultureToUse = culture ?? Culture;
            return UnwrapPossibleArrayType(cultureToUse, value, type);
        }

        private object UnwrapPossibleArrayType(CultureInfo culture, object value, Type destinationType)
        {
            // array conversion results in four cases, as below
            var valueAsArray = value as Array;
            if (destinationType.IsArray)
            {
                var destinationElementType = destinationType.GetElementType();
                if (valueAsArray != null)
                {
                    // case 1: both destination + source type are arrays, so convert each element
                    var converted = (IList)Array.CreateInstance(destinationElementType, valueAsArray.Length);
                    for (var i = 0; i < valueAsArray.Length; i++)
                    {
                        converted[i] = ConvertSimpleType(culture, valueAsArray.GetValue(i), destinationElementType);
                    }
                    return converted;
                }
                else
                {
                    // case 2: destination type is array but source is single element, so wrap element in
                    // array + convert
                    var element = ConvertSimpleType(culture, value, destinationElementType);
                    var converted = (IList)Array.CreateInstance(destinationElementType, 1);
                    converted[0] = element;
                    return converted;
                }
            }
            else if (valueAsArray != null)
            {
                // case 3: destination type is single element but source is array, so extract first element + convert
                if (valueAsArray.Length > 0)
                {
                    value = valueAsArray.GetValue(0);
                    return ConvertSimpleType(culture, value, destinationType);
                }
                else
                {
                    // case 3(a): source is empty array, so can't perform conversion
                    return null;
                }
            }

            // case 4: both destination + source type are single elements, so convert
            return ConvertSimpleType(culture, value, destinationType);
        }

        private object ConvertSimpleType(CultureInfo culture, object value, Type destinationType)
        {
            if (value == null || value.GetType().IsAssignableFrom(destinationType))
            {
                return value;
            }

            // In case of a Nullable object, we try again with its underlying type.
            destinationType = UnwrapNullableType(destinationType);

            // if this is a user-input value but the user didn't type anything, return no value
            var valueAsString = value as string;
            if (valueAsString != null && string.IsNullOrWhiteSpace(valueAsString))
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(destinationType);
            var canConvertFrom = converter.CanConvertFrom(value.GetType());
            if (!canConvertFrom)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!(canConvertFrom || converter.CanConvertTo(destinationType)))
            {
                // EnumConverter cannot convert integer, so we verify manually
                if (destinationType.GetTypeInfo().IsEnum &&
                    (value is int ||
                    value is uint ||
                    value is long ||
                    value is ulong ||
                    value is short ||
                    value is ushort ||
                    value is byte ||
                    value is sbyte))
                {
                    return Enum.ToObject(destinationType, value);
                }

                throw new InvalidOperationException(
                    Resources.FormatValueProviderResult_NoConverterExists(value.GetType(), destinationType));
            }

            try
            {
                return canConvertFrom
                           ? converter.ConvertFrom(null, culture, value)
                           : converter.ConvertTo(null, culture, value, destinationType);
            }
            catch (Exception ex)
            {
                if (ex is FormatException)
                {
                    throw ex;
                }
                else
                {
                    // TypeConverter throws System.Exception wrapping the FormatException,
                    // so we throw the inner exception.
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

                    // this code is never reached because the previous line is throwing;
                    throw;
                }
            }
        }

        private static Type UnwrapNullableType(Type destinationType)
        {
            return Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        }
    }
}
