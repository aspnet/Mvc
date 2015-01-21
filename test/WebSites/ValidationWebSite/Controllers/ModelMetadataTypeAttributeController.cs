// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using ValidationWebSite.ViewModels;

namespace ValidationWebSite.Controllers
{
    public class ModelMetadataTypeValidationController : Controller
    {
        [HttpPost]
        public object ValidateProductViewModelInclMetadata([FromBody] ProductViewModel product)
        {
            return CreateValidationDictionary();
        }

        [HttpPost]
        public object ValidateSoftwareViewModelInclMetadata([FromBody] SoftwareViewModel software)
        {
            return CreateValidationDictionary();
        }

        [HttpPost]
        public object TryValidateModelProductViewModelWithErrorInParameter(int id, [FromBody] ProductViewModel product)
        {
            if (id == 0)
            {
                ModelState["id"].Errors.Clear();
            }
            TryValidateModel(product);

            return CreateValidationDictionary();
        }

        [HttpGet]
        public object TryValidateModelSoftwareViewModelNoPrefix()
        {
            var softwareVm = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "UK",
                DatePurchased = new DateTime(10, 10, 10),
                Name = "MVC",
                Price = 110,
                Version = "2"
            };

            TryValidateModel(softwareVm);

            return CreateValidationDictionary();
        }

        [HttpGet]
        public object TryValidateModelValidModelNoPrefix()
        {
            var softwareVm = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "USA",
                DatePurchased = new DateTime(10, 10, 10),
                Name = "MVC",
                Price = 110,
                Version = "2"
            };

            TryValidateModel(softwareVm);

            return CreateValidationDictionary();
        }

        private Dictionary<string, string> CreateValidationDictionary()
        {
            var result = new Dictionary<string, string>();
            foreach (var item in ModelState)
            {
                var error = item.Value.Errors.SingleOrDefault();
                if (error != null)
                {
                    var value = error.Exception != null ? error.Exception.Message :
                                                          error.ErrorMessage;
                    result.Add(item.Key, value);
                }
            }

            return result;
        }

    }
}
