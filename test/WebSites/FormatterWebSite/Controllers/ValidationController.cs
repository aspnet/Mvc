﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace FormatterWebSite
{
    public class ValidationController : Controller
    {
        [HttpPost]
        public IActionResult Index([FromBody]User user)
        {
            if (!ModelState.IsValid)
            {
                return Content(ModelState["user.Id"].Errors[0].ErrorMessage + "," +
                    ModelState["user.Name"].Errors[0].ErrorMessage + "," +
                    ModelState["user.Alias"].Errors[0].ErrorMessage + "," +
                    ModelState["user.Designation"].Errors[0].ErrorMessage);
            }

            return Content("User has been registerd : " + user.Name);
        }

        [HttpPost]
        public string GetDeveloperName([FromBody] Developer developer)
        {
            // Developer is excluded in startup, hence the value should never be passed.
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(developer.Name))
                {
                    return "No model validation for developer, even though developer.Name is empty.";
                }

                return developer.Name;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [HttpPost]
        public string GetDeveloperAlias(Developer developer)
        {
            // Since validation exclusion is currently only effective in case of body bound models.
            if (ModelState.IsValid)
            {
                return developer.Alias;
            }
            else
            {
                return ModelState["Name"].Errors[0].ErrorMessage;
            }
        }

        // 'Developer' type is excluded but the shallow validation on the 
        // property Developers should happen
        [ModelStateValidationFilter]
        public IActionResult CreateProject([FromBody]Project project)
        {
            return Json(project);
        }

        [ModelStateValidationFilter]
        public IActionResult CreateSimpleTypePropertiesModel([FromBody] SimpleTypePropertiesModel simpleTypePropertiesModel)
        {
            return Json(simpleTypePropertiesModel);
        }
    }
}