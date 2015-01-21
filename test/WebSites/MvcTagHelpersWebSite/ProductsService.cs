// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.Expiration.Interfaces;

namespace MvcTagHelpersWebSite
{
    public class ProductsService
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public string GetProducts(string category, out IExpirationTrigger trigger)
        {
            trigger = new CancellationTokenTrigger(_tokenSource.IsCancellationRequested ? CancellationToken.None :
                                                                                          _tokenSource.Token);

            if (category == "Books")
            {
                return "Book1, Book2";
            }
            else
            {
                return "Laptops";
            }
        }

        public void UpdateProducts()
        {
            _tokenSource.Cancel();
        }
    }
}