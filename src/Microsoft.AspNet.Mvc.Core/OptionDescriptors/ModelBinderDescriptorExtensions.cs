﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.OptionDescriptors;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Extension methods for adding model binders to a collection.
    /// </summary>
    public static class ModelBinderDescriptorExtensions
    {
        /// <summary>
        /// Adds a type representing a <see cref="IModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors</param>
        /// <param name="modelBinderType">Type representing an <see cref="IModelBinder"/>.</param>
        /// <returns>ModelBinderDescriptor representing the added instance.</returns>
        public static ModelBinderDescriptor Add([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                [NotNull] Type modelBinderType)
        {
            var descriptor = new ModelBinderDescriptor(modelBinderType);
            descriptors.Add(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Inserts a type representing a <see cref="IModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors</param>
        /// <param name="modelBinderType">Type representing an <see cref="IModelBinder"/>.</param>
        /// <returns>ModelBinderDescriptor representing the inserted instance.</returns>
        public static ModelBinderDescriptor Insert([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                   int index,
                                                   [NotNull] Type modelBinderType)
        {
            if (index < 0 || index > descriptors.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new ModelBinderDescriptor(modelBinderType);
            descriptors.Insert(index, descriptor);
            return descriptor;
        }

        /// <summary>
        /// Adds an <see cref="IModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors</param>
        /// <param name="modelBinder">An <see cref="IModelBinder"/> instance.</param>
        /// <returns>ModelBinderDescriptor representing the added instance.</returns>
        public static ModelBinderDescriptor Add([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                [NotNull] IModelBinder modelBinder)
        {
            var descriptor = new ModelBinderDescriptor(modelBinder);
            descriptors.Add(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Insert an <see cref="IModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors</param>
        /// <param name="modelBinder">An <see cref="IModelBinder"/> instance.</param>
        /// <returns>ModelBinderDescriptor representing the added instance.</returns>
        public static ModelBinderDescriptor Insert([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                   int index,
                                                   [NotNull] IModelBinder modelBinder)
        {
            if (index < 0 || index > descriptors.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new ModelBinderDescriptor(modelBinder);
            descriptors.Insert(index, descriptor);
            return descriptor;
        }

        /// <summary>
        /// Adds a <see cref="TypeSpecificModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors.</param>
        /// <param name="type">The model type that the given instance should bind with.</param>
        /// <param name="modelBinder">An <see cref="IModelBinder"/> instance.</param>
        /// <returns>The <see cref="ModelBinderDescriptor"/> representing the added instance.</returns>
        public static ModelBinderDescriptor Add([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                [NotNull] Type type,
                                                [NotNull] IModelBinder modelBinder)
        {
            var typeModelBinder = new TypeSpecificModelBinder(type, modelBinder);
            var descriptor = new ModelBinderDescriptor(typeModelBinder);
            descriptors.Add(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Adds a <see cref="TypeSpecificModelBinder"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of ModelBinderDescriptors.</param>
        /// <param name="type">The model type that the given instance should bind with.</param>
        /// <param name="modelBinderProvider">An <see cref="IModelBinderProvider"/> instance.</param>
        /// <returns>The <see cref="ModelBinderDescriptor"/> representing the added instance.</returns>
        public static ModelBinderDescriptor Add([NotNull] this IList<ModelBinderDescriptor> descriptors,
                                                [NotNull] Type type,
                                                [NotNull] IModelBinderProvider modelBinderprovider)
        {
            var compositeBinder = new CompositeModelBinder(modelBinderprovider);
            var typeModelBinder = new TypeSpecificModelBinder(type, compositeBinder);
            var descriptor = new ModelBinderDescriptor(typeModelBinder);
            descriptors.Add(descriptor);
            return descriptor;
        }
    }
}