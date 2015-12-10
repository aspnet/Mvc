// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace HtmlGenerationWebSite
{
    public class ProductsService
    {
        public string GetProducts(string category)
        {
            if (category == "Books")
            {
                return "Book1, Book2";
            }
            else
            {
                return "Laptops";
            }
        }
    }
}