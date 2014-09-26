// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.ReflectedModelBuilder
{
    /// <summary>
    /// Allows customization of the of the <see cref="ReflectedControllerModel"/>.
    /// </summary>
    /// <remarks>
    /// To use this interface, create an <see cref="System.Attribute"/> class which implements the interface and
    /// place it on a controller class.
    /// 
    /// <see cref="IReflectedControllerModelConvention"/> customizations run after 
    /// <see cref="IReflectedApplicationModelConvention"/> customications and before 
    /// <see cref="IReflectedActionModelConvention"/> customizations.
    /// </remarks>
    public interface IReflectedControllerModelConvention
    {
        /// <summary>
        /// Called when <see cref="ReflectedControllerModel"/> is created.
        /// </summary>
        /// <param name="model">The <see cref="ReflectedControllerModel"/>.</param>
        void OnModelCreated([NotNull] ReflectedControllerModel model);
    }
}