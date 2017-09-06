// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// A context for exception filters i.e. <see cref="IExceptionFilter"/> and
    /// <see cref="IAsyncExceptionFilter"/> implementations.
    /// </summary>
    public class ExceptionContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        /// <summary>
        /// Instantiates a new <see cref="ExceptionContext"/> instance.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="filters">All applicable <see cref="IFilterMetadata"/> implementations.</param>
        public ExceptionContext(ActionContext actionContext, IList<IFilterMetadata> filters)
            : base(actionContext, filters)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Exception"/> caught while executing the action.
        /// </summary>
        public virtual Exception Exception
        {
            get
            {
                if (_exception == null && _exceptionDispatchInfo != null)
                {
                    return _exceptionDispatchInfo.SourceException;
                }
                else
                {
                    return _exception;
                }
            }

            set
            {
                _exceptionDispatchInfo = null;
                _exception = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo"/> for the
        /// <see cref="Exception"/>, if this information was captured.
        /// </summary>
        public virtual ExceptionDispatchInfo ExceptionDispatchInfo
        {
            get
            {
                return _exceptionDispatchInfo;
            }

            set
            {
                _exception = null;
                _exceptionDispatchInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets an indication that the <see cref="Exception"/> has been handled.
        /// </summary>
        public virtual bool ExceptionHandled { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="IActionResult"/>.
        /// </summary>
        public virtual IActionResult Result { get; set; }
    }
}
