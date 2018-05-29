// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;

namespace BasicApi.Controllers
{
    [Route("/store")]
    public class StoreController : ControllerBase
    {
        [HttpGet("inventory")]
        public IActionResult GetInventory()
        {
            throw new NotImplementedException();
        }

        [HttpGet("order/{orderId}")]
        public IActionResult FindOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        [HttpPost("order")]
        public IActionResult PlaceOrder()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("order/{orderId}")]
        public IActionResult CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }
    }
}
