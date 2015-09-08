// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Actions;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ActionResults
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that when executed will
    /// do nothing.
    /// </summary>
    public class EmptyResult : ActionResult
    {
        private static readonly EmptyResult _singleton = new EmptyResult();

        internal static EmptyResult Instance
        {
            get { return _singleton; }
        }

        /// <inheritdoc />
        public override void ExecuteResult([NotNull] ActionContext context)
        {
        }
    }
}
