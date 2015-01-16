// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using ModelBindingWebSite.ViewModels;

namespace ModelBindingWebSite.Controllers
{
    public class ModelMetadataTypeAttributeController : Controller
    {
        [HttpPost]
        public object ValidateProductViewModelInclMetadata([FromBody]ProductViewModel product)
        {
            return CreateValidationDictionary();
        }

        [HttpPost]
        public object ValidateSoftwareViewModelInclMetadata([FromBody]SoftwareViewModel software)
        {
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
