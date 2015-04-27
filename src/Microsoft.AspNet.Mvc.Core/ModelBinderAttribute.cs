﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An attribute that can specify a model name or type of <see cref="IModelBinder"/> to use for binding.
    /// </summary>
    [AttributeUsage(

        // Support method parameters in actions.
        AttributeTargets.Parameter |

        // Support properties on model DTOs.
        AttributeTargets.Property |

        // Support model types.
        AttributeTargets.Class |
        AttributeTargets.Enum |
        AttributeTargets.Struct,

        AllowMultiple = false,
        Inherited = true)]
    public class ModelBinderAttribute : Attribute, IModelNameProvider, IBinderTypeProviderMetadata
    {
        private BindingSource _bindingSource;

        /// <inheritdoc />
        public Type BinderType { get; set; }

        /// <inheritdoc />
        public BindingSource BindingSource
        {
            get
            {
                if (_bindingSource == null && BinderType != null)
                {
                    return BindingSource.Custom;
                }

                return _bindingSource;
            }
            set
            {
                _bindingSource = value;
            }
        }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}