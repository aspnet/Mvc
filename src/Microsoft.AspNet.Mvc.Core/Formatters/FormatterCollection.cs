// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class FormatterCollection<TFormatter> : List<TFormatter>
    {
        /// <summary>
        /// Removes all formatters of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to remove.</typeparam>
        public void RemoveType<T>() where T : TFormatter
        {
            RemoveAll(x => x is T);
        }
    }
}
