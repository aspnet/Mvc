// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal
{
    public interface IXmlResultExecutor
    {
        /// <summary>
        /// Executes the <see cref="XmlResult"/> and writes the response.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="result">The <see cref="XmlResult"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when writing has completed.</returns>
        Task ExecuteAsync(ActionContext context, XmlResult result);
    }
}