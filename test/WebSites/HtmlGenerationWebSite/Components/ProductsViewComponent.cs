// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using HtmlGenerationWebSite.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HtmlGenerationWebSite.Components
{
    public class ProductsViewComponent : ViewComponent
    {
        public ProductsViewComponent(
            ProductsService productsService,
            TokenProviderService tokenService,
            IMemoryCache cache)
        {
            ProductsService = productsService;
            TokenService = tokenService;
            Cache = cache;
        }

        private ProductsService ProductsService { get; }

        private TokenProviderService TokenService { get; }

        public IMemoryCache Cache { get; }

        public IViewComponentResult Invoke(string category)
        {
            string products;
            if (!Cache.TryGetValue(category, out products))
            {
                var changeToken = TokenService.GetToken(typeof(Product));
                products = Cache.Set<string>(
                    category,
                    ProductsService.GetProducts(category),
                    new MemoryCacheEntryOptions().AddExpirationToken(changeToken));
            }

            ViewData["Products"] = products;
            return View();
        }
    }
}