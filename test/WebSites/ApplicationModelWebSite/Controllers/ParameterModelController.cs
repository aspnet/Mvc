﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ApplicationModelWebSite
{
    // This controller uses an reflected model attribute to change a parameter's binder metadata.
    //
    // This could be accomplished by simply making an attribute that implements IBinderMetadata, but
    // this is part of a test for IParameterModelConvention.
    public class ParameterModelController : Controller
    {
        public string GetParameterMetadata([Cool] int? id)
        {
            return ActionContext.ActionDescriptor.Parameters[0].BindingInfo.BinderModelName;
        }

        private class CoolAttribute : Attribute, IParameterModelConvention
        {
            public void Apply(ParameterModel model)
            {
                model.BindingInfo.BindingSource = BindingSource.Custom;
                model.BindingInfo.BinderModelName = "CoolMetadata";
            }
        }
    }
}