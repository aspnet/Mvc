// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// Represents a collection of formatters.
    /// </summary>
    /// <typeparam name="TFormatter">The type of formatters in the collection.</typeparam>
    public class FormatterCollection<TFormatter> : Collection<TFormatter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterCollection{TFormatter}"/> class that is empty.
        /// </summary>
        public FormatterCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterCollection{TFormatter}"/> class
        /// as a wrapper for the specified list.
        /// </summary>
        /// <param name="list">The list that is wrapped by the new collection.</param>
        public FormatterCollection(IList<TFormatter> list)
            : base(list)
        {
        }

        /// <summary>
        /// Removes all formatters of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to remove.</typeparam>
        public void RemoveType<T>() where T : TFormatter
        {
            RemoveType(typeof(T));
        }

        /// <summary>
        /// Removes all formatters of the specified type.
        /// </summary>
        /// <param name="formatterType">The type to remove.</param>
        public void RemoveType(Type formatterType)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var formatter = this[i];
                if (formatter.GetType() == formatterType)
                {
                    RemoveAt(i);
                }
            }
        }
    }
}
