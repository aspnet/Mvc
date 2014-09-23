﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using System.Threading.Tasks;

namespace ApiExplorer
{
    [Route("ApiExplorerResponseTypeWithoutAttribute/[Action]")]
    public class ApiExplorerResponseTypeWithoutAttributeController : Controller
    {
        [HttpGet]
        public void GetVoid()
        {
        }

        [HttpGet]
        public object GetObject()
        {
            return null;
        }

        [HttpGet]
        public IActionResult GetIActionResult()
        {
            return new EmptyResult();
        }

        [HttpGet]
        public ObjectResult GetDerivedActionResult()
        {
            return new ObjectResult(null);
        }

        [HttpGet]
        public Product GetProduct()
        {
            return null;
        }

        [HttpGet]
        public int GetInt()
        {
            return 0;
        }

        [HttpGet]
        public Task GetTask()
        {
            return Task.FromResult<object>(null);
        }

        [HttpGet]
        public Task<object> GetTaskOfObject()
        {
            return Task.FromResult<object>(null);
        }

        [HttpGet]
        public Task<IActionResult> GetTaskOfIActionResult()
        {
            return Task.FromResult<IActionResult>(new EmptyResult());
        }

        [HttpGet]
        public Task<ObjectResult> GetTaskOfDerivedActionResult()
        {
            return Task.FromResult<ObjectResult>(new ObjectResult(null));
        }

        [HttpGet]
        public Task<Product> GetTaskOfProduct()
        {
            return Task.FromResult<Product>(null);
        }

        [HttpGet]
        public Task<int> GetTaskOfInt()
        {
            return Task.FromResult<int>(0);
        }
    }
}