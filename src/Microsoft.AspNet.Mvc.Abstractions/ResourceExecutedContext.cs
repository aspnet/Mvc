// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A context for resource filters.
    /// </summary>
    public class ResourceExecutedContext : FilterContext
    {
        private Exception _exception;
        private ExceptionDispatchInfo _exceptionDispatchInfo;

        /// <summary>
        /// Creates a new <see cref="ResourceExecutedContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="filters">The list of <see cref="IFilterMetadata"/> instances.</param>
        public ResourceExecutedContext(ActionContext actionContext, IList<IFilterMetadata> filters)
            : base(actionContext, filters)
        {
        }

        /// <summary>
        /// Gets or sets a value which indicates whether or not execution was canceled by a resource filter.
        /// If true, then a resource filter short-circuted execution by setting
        /// <see cref="ResourceExecutingContext.Result"/>.
        /// </summary>
        public virtual bool Canceled { get; set; }

        /// <summary>
        /// Gets or set the current <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Exception"/> or <see cref="ExceptionDispatchInfo"/> to <c>null</c> will treat
        /// the exception as handled, and it will not be rethrown by the runtime.
        /// 
        /// Setting <see cref="ExceptionHandled"/> to <c>true</c> will also mark the exception as handled.
        /// </remarks>
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
        /// Gets or set the current <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Exception"/> or <see cref="ExceptionDispatchInfo"/> to <c>null</c> will treat
        /// the exception as handled, and it will not be rethrown by the runtime.
        /// 
        /// Setting <see cref="ExceptionHandled"/> to <c>true</c> will also mark the exception as handled.
        /// </remarks>
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
        /// Gets or sets a value indicating whether or not the current <see cref="Exception"/> has been handled.
        /// 
        /// If <c>false</c> the <see cref="Exception"/> will be rethrown by the runtime after resource filters
        /// have executed.
        /// </summary>
        public virtual bool ExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <remarks>
        /// The <see cref="Result"/> may be provided by execution of the action itself or by another
        /// filter. 
        /// 
        /// The <see cref="Result"/> has already been written to the response before being made available
        /// to resource filters.
        /// </remarks>
        public virtual IActionResult Result { get; set; }
    }
}