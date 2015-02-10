// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite
{
    public class Employee : Person
    {
        public string Department { get; set; }

        public string Location { get; set; }

        [FromQuery(Name = "EmployeeId")]
        public int Id { get; set; }

        [FromRoute(Name = "EmployeeTaxId")]
        public int TaxId { get; set; }

        [FromForm(Name = "EmployeeSSN")]
        public string SSN { get; set; }
    }
}