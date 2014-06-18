// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents a collection of ModelBinderDescriptors.
    /// </summary>
    public class ModelBinderDescriptorCollection : Collection<ModelBinderDescriptor>
    {
        public ModelBinderDescriptor Add([NotNull] Type modelBinderType)
        {
            var descriptor = new ModelBinderDescriptor(modelBinderType);
            Add(descriptor);
            return descriptor;
        }

        public ModelBinderDescriptor Insert(int index, [NotNull] Type modelBinderType)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new ModelBinderDescriptor(modelBinderType);
            Insert(index, descriptor);
            return descriptor;
        }

        public ModelBinderDescriptor Add([NotNull] IModelBinder modelBinder)
        {
            var descriptor = new ModelBinderDescriptor(modelBinder);
            Add(descriptor);
            return descriptor;
        }

        public ModelBinderDescriptor Insert(int index, [NotNull] IModelBinder modelBinder)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new ModelBinderDescriptor(modelBinder);
            Insert(index, descriptor);
            return descriptor;
        }
    }
}