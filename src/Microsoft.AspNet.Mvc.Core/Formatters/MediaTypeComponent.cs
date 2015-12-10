// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.Formatters
{
    /// <summary>
    /// Represents a component within a media type.
    /// </summary>
    public struct MediaTypeComponent : IEquatable<MediaTypeComponent>
    {
        /// <summary>
        /// The name of the type in a media type when represented as a <see cref="MediaTypeComponent"/>.
        /// </summary>
        public static readonly StringSegment Type = new StringSegment("type");

        /// <summary>
        /// The name of the subtype in a media type when represented as a <see cref="MediaTypeComponent"/>.
        /// </summary>
        public static readonly StringSegment Subtype = new StringSegment("subtype");

        /// <summary>
        /// Initializes a <see cref="MediaTypeComponent"/>.
        /// </summary>
        /// <param name="name">The name of the component of the media type.</param>
        /// <param name="value">The value of the component of the media type.</param>
        public MediaTypeComponent(StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="StringSegment"/> with the name of the <see cref="MediaTypeComponent"/>.
        /// </summary>
        public StringSegment Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StringSegment"/> with the value of the <see cref="MediaTypeComponent"/>.
        /// </summary>
        public StringSegment Value { get; set; }

        /// <summary>
        /// Determines whether this <see cref="MediaTypeComponent"/> represents a MatchesAll value.
        /// </summary>
        /// <returns><code>true</code>if the component is a type or subtype <see cref="MediaTypeComponent"/>
        /// and its value is match all; otherwise <code>false</code>.</returns>
        public bool IsMatchesAll()
        {
            return (Name == Type || Name == Subtype) &&
                Value.Equals("*", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the current <see cref="MediaTypeComponent"/> has the given name. The comparison is
        /// case insensitive.
        /// </summary>
        /// <param name="name">The <paramref name="name"/> to compare to.</param>
        /// <returns><code>true</code>if the name of this <see cref="MediaTypeComponent"/> is equal to the given
        /// <paramref name="name"/>; otherwise <code>false</code>.</returns>
        public bool HasName(string name)
        {
            return HasName(new StringSegment(name));
        }

        /// <summary>
        /// Determines if the current <see cref="MediaTypeComponent"/> has the given name. The comparison is
        /// case insensitive.
        /// </summary>
        /// <param name="name">The <paramref name="name"/> to compare to.</param>
        /// <returns><code>true</code>if the name of this <see cref="MediaTypeComponent"/> is equal to the given
        /// <paramref name="name"/>; otherwise <code>false</code>.</returns>
        public bool HasName(StringSegment name)
        {
            return Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the current <see cref="MediaTypeComponent"/> has the given value. The comparison is
        /// case insensitive.
        /// </summary>
        /// <param name="value">The <paramref name="value"/> to compare to.</param>
        /// <returns><code>true</code>if the value of this <see cref="MediaTypeComponent"/> is equal to the given
        /// <paramref name="value"/>; otherwise <code>false</code>.</returns>
        public bool HasValue(string value)
        {
            return HasValue(new StringSegment(value));
        }

        /// <summary>
        /// Determines if the current <see cref="MediaTypeComponent"/> has the given value. The comparison is
        /// case insensitive.
        /// </summary>
        /// <param name="value">The <paramref name="value"/> to compare to.</param>
        /// <returns><code>true</code>if the value of this <see cref="MediaTypeComponent"/> is equal to the given
        /// <paramref name="value"/>; otherwise <code>false</code>.</returns>
        public bool HasValue(StringSegment value)
        {
            return Value.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><code>true</code> if the current object is equal to the other parameter; otherwise, <code>false</code>.</returns>
        public bool Equals(MediaTypeComponent other)
        {
            return HasName(other.Name) && HasValue(other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MediaTypeComponent && Equals((MediaTypeComponent)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCodeCombiner hashCode = HashCodeCombiner.Start();
            hashCode.Add(Name.Value);
            hashCode.Add(Value.Value);
            return  hashCode;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name}={Value}";
    }
}