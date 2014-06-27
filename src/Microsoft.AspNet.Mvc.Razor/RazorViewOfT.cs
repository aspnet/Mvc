// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class RazorView<TModel> : RazorView
    {
        public TModel Model
        {
            get
            {
                return ViewData == null ? default(TModel) : ViewData.Model;
            }
        }

        public ViewDataDictionary<TModel> ViewData { get; private set; }

        public override Task RenderAsync([NotNull] ViewContext context)
        {
            Activate(context);
            return base.RenderAsync(context);
        }

        private void Activate(ViewContext context)
        {
            ViewData = context.ViewData as ViewDataDictionary<TModel>;
            if (ViewData == null)
            {
                if (context.ViewData != null)
                {
                    ViewData = new ViewDataDictionary<TModel>(context.ViewData);
                }
                else
                {
                    var metadataProvider = GetService<IModelMetadataProvider>(context);
                    ViewData = new ViewDataDictionary<TModel>(metadataProvider);
                }

                // Have new ViewDataDictionary; make sure it's visible everywhere.
                context.ViewData = ViewData;
            }

            var viewActivator = GetService<IRazorViewActivator>(context);
            viewActivator.Activate(this, context);
        }

        private static TService GetService<TService>(ViewContext context)
        {
            return context.HttpContext.RequestServices.GetService<TService>();
        }
    }
}
