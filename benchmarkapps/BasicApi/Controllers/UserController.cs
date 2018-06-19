// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;

namespace BasicApi.Controllers
{
    [Route("/user")]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateUser()
        {
            throw new NotImplementedException();
        }

        [HttpPost("createWithArray")]
        public IActionResult CreateUserWithArray()
        {
            throw new NotImplementedException();
        }

        [HttpPost("createWithList")]
        public IActionResult CreateUserWithList()
        {
            throw new NotImplementedException();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            throw new NotImplementedException();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{username}")]
        public IActionResult UpdateUser(string username)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            throw new NotImplementedException();
        }
    }
}
