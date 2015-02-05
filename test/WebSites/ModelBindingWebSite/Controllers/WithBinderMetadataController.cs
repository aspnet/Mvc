﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite.Controllers
{
    public class WithBinderMetadataController : Controller
    {
        public EmployeeWithBinderMetadata BindWithTypeMetadata(EmployeeWithBinderMetadata emp)
        {
            return emp;
        }

        public DerivedEmployee TypeMetadataAtDerivedTypeWinsOverTheBaseType(DerivedEmployee emp)
        {
            return emp;
        }

        public void ParameterMetadataOverridesTypeMetadata([FromBody] Employee emp)
        {
        }

        public Employee ParametersWithNoValueProviderMetadataUseTheAvailableValueProviders([FromQuery] Employee emp)
        {
            return emp;
        }

        public Document EchoDocument(Document poco)
        {
            return poco;
        }
    }
}