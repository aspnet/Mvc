// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// The context associated with the current request for a Razor page.
    /// </summary>
    public class PageContext : ActionContext
    {
        private CompiledPageActionDescriptor _actionDescriptor;
        private Page _page;
        private ViewContext _viewContext;
        private IList<IValueProviderFactory> _valueProviderFactories;

        /// <summary>
        /// Creates an empty <see cref="PageContext"/>.
        /// </summary>
        /// <remarks>
        /// The default constructor is provided for unit test purposes only.
        /// </remarks>
        public PageContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PageContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        public PageContext(ActionContext actionContext)
            : base(actionContext)
        {
        }
        

        /// <summary>
        /// Gets or sets the <see cref="PageActionDescriptor"/>.
        /// </summary>
        public new CompiledPageActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                _actionDescriptor = value;
                base.ActionDescriptor = value;
            }
        }

        public virtual Page Page
        {
            get { return _page; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _page = value;
            }
        }

        public virtual ViewContext ViewContext
        {
            get
            {
                return _viewContext;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _viewContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the applicable _ViewStart instances.
        /// </summary>
        public IReadOnlyList<IRazorPage> ViewStarts { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="IValueProviderFactory"/> instances for the current request.
        /// </summary>
        public virtual IList<IValueProviderFactory> ValueProviderFactories
        {
            get
            {
                if (_valueProviderFactories == null)
                {
                    _valueProviderFactories = new List<IValueProviderFactory>();
                }

                return _valueProviderFactories;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueProviderFactories = value;
            }
        }
    }
}