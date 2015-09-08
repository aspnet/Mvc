// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionResults;

namespace ActionResultsWebSite
{
    public class ActionResultsVerificationController : Controller
    {

        [FromServices]
        public GuidLookupService Service { get; set; }

        public IActionResult Index([FromBody]DummyClass test)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            return Content("Hello World!");
        }

        public IActionResult GetBadResult()
        {
            return new BadRequestResult();
        }

        public IActionResult GetCreatedRelative()
        {
            return Created("1", CreateDummy());
        }

        public IActionResult GetCreatedAbsolute()
        {
            return Created("/ActionResultsVerification/GetDummy/1", CreateDummy());
        }

        public IActionResult GetCreatedQualified()
        {
            return Created("http://localhost/ActionResultsVerification/GetDummy/1", CreateDummy());
        }

        public IActionResult GetCreatedUri()
        {
            return Created(new Uri("/ActionResultsVerification/GetDummy/1", UriKind.Relative), CreateDummy());
        }

        public IActionResult GetCreatedAtAction()
        {
            var values = new { id = 1 };
            return CreatedAtAction("GetDummy", "ActionResultsVerification", values, CreateDummy());
        }

        public IActionResult GetCreatedAtRoute()
        {
            var values = new { controller = "ActionResultsVerification", Action = "GetDummy", id = 1 };
            return CreatedAtRoute(null, values, CreateDummy());
        }

        public IActionResult GetCreatedAtRouteWithRouteName()
        {
            var values = new { controller = "ActionResultsVerification", Action = "GetDummy", id = 1 };
            return CreatedAtRoute("custom-route", values, CreateDummy());
        }

        public IActionResult GetContentResult()
        {
            return Content("content");
        }

        public IActionResult GetContentResultWithContentType()
        {
            return Content("content", "application/json");
        }

        public IActionResult GetContentResultWithContentTypeAndEncoding()
        {
            return Content("content", "application/json", Encoding.ASCII);
        }

        public IActionResult GetObjectResultWithNoContent()
        {
            var result = new ObjectResult(null);
            result.StatusCode = StatusCodes.Status201Created;
            return result;
        }

        public IActionResult GetNotFoundObjectResult()
        {
            return HttpNotFound(null);
        }

        public IActionResult GetNotFoundObjectResultWithContent()
        {
            return HttpNotFound(CreateDummy());
        }

        public IActionResult GetNotFoundObjectResultWithDisposableObject(string guid)
        {
            return HttpNotFound(CreateDisposableType(guid));
        }

        public bool GetDisposeCalled(string guid)
        {
            bool value;
            if (Service.IsDisposed.TryGetValue(guid, out value))
            {
                return value;
            }

            return false;
        }

        public DummyClass GetDummy(int id)
        {
            return CreateDummy();
        }

        private DummyClass CreateDummy()
        {
            return new DummyClass()
            {
                SampleInt = 10,
                SampleString = "Foo"
            };
        }

        private DisposableType CreateDisposableType(string guid)
        {
            return new DisposableType(Service, guid);
        }

        private class DisposableType : IDisposable
        {
            private GuidLookupService _service;
            private string _guid;

            public DisposableType(GuidLookupService service, string guid)
            {
                _service = service;
                _guid = guid;
                _service.IsDisposed[_guid] = false;
            }

            public void Dispose()
            {
                _service.IsDisposed[_guid] = true;
            }
        }
    }
}