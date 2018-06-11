// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class GetResponseMetadata_ControllerWithoutConvention : ControllerBase
    {
        public ActionResult<Person> GetPerson(int id) => null;

        public ActionResult<Person> PostPerson(Person person) => null;
    }

    public class GetResponseMetadata_ControllerActionWithAttributes : ControllerBase
    {
        [Produces(typeof(Person))]
        public IActionResult ActionWithPrducesAttribute(int id) => null;

        [ProducesResponseType(201)]
        public IActionResult ActionWithProducesResponseType_StatusCodeInConstructor() => null;

        [ProducesResponseType(typeof(Person), 202)]
        public IActionResult ActionWithProducesResponseType_StatusCodeAndTypeInConstructor() => null;

        [ProducesResponseType(200, StatusCode = 203)]
        public IActionResult ActionWithProducesResponseType_StatusCodeInConstructorAndProperty() => null;

        [ProducesResponseType(typeof(object), 200, Type = typeof(Person), StatusCode = 201)]
        public IActionResult ActionWithProducesResponseType_StatusCodeAndTypeInConstructorAndProperty() => null;

        [CustomResponseType(Type = typeof(Person), StatusCode = 204)]
        public IActionResult ActionWithCustomApiResponseMetadataProvider() => null;

        [CustomResponseTypeDerived(Type = typeof(Person), StatusCode = "204")]
        public IActionResult ActionWithCustomApiResponseMetadataProviderWithIncorrectStatusCodeType() => null;
    }

    public class Person { }

    public class CustomResponseTypeAttribute : Attribute, IApiResponseMetadataProvider
    {
        public Type Type { get; set; }

        public int StatusCode { get; set; }

        public void SetContentTypes(MediaTypeCollection contentTypes)
        {
        }
    }

    public class CustomResponseTypeDerivedAttribute : CustomResponseTypeAttribute
    {
        private string _statusCode;

        public new string StatusCode
        {
            get => _statusCode;
            set
            {
                _statusCode = value;
                base.StatusCode = int.Parse(value);
            }
        }
    }
}
