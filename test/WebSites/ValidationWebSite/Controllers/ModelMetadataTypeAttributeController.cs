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
        public object ValidateProductViewModelIncludingMetadata([FromBody] ProductViewModel product)
        {
            return CreateValidationDictionary();
        }

        [HttpPost]
        public object ValidateSoftwareViewModelIncludingMetadata([FromBody] SoftwareViewModel software)
        {
            return CreateValidationDictionary();
        }

        [HttpPost]
        public object TryValidateModelProductViewModelWithErrorInParameter(int id, [FromBody] ProductViewModel product)
        {
            //Clear ModelState entry if id is 0. TryValidateModel should not add entries that are cleared.
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
            var softwareViewModel = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "UK",
                DatePurchased = new DateTime(2010, 10, 10),
                Price = 110,
                Version = "2"
            };

            TryValidateModel(softwareViewModel, "software");

            return CreateValidationDictionary();
        }

        [HttpGet]
        public object TryValidateModelValidModelNoPrefix()
        {
            var softwareViewModel = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "USA",
                DatePurchased = new DateTime(2010, 10, 10),
                Name = "MVC",
                Price = 110,
                Version = "2"
            };

            TryValidateModel(softwareViewModel);

            return CreateValidationDictionary();
        }

        private Dictionary<string, string> CreateValidationDictionary()
        {
            var result = new Dictionary<string, string>();
            foreach (var item in ModelState)
            {
                var errorMessage = string.Empty;
                foreach (var error in item.Value.Errors)
                {
                    if (error != null)
                    {
                        errorMessage = errorMessage + error.ErrorMessage;
                    }
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    result.Add(item.Key, errorMessage);
                }
            }

            return result;
        }
    }
}
