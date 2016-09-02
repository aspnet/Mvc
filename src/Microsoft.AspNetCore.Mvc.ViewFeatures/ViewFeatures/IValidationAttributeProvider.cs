// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Contract for a service providing validation attributes for expressions.
    /// </summary>
    public interface IValidationAttributeProvider
    {
        /// <summary>
        /// Adds validation attributes to the <paramref name="attributes" /> if client validation is enabled.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> for the <paramref name="expression"/>.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="attributes">
        /// The <see cref="Dictionary{TKey, TValue}"/> to receive the validation attributes. Maps the validation
        /// attribute names to their <see cref="string"/> values. Values must be HTML encoded before they are written
        /// to an HTML document or response.
        /// </param>
        /// <remarks>
        /// Adds nothing to <paramref name="attributes"/> if client-side validation is disabled or if attributes have
        /// already been generated for the <paramref name="expression"/> in the current &lt;form&gt;.
        /// </remarks>
        void AddValidationAttributes(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            IDictionary<string, string> attributes);
    }
}