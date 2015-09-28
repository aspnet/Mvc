// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    /// <summary>
    /// A type which is used to represent a property in a <see cref="ControllerModel"/>.
    /// </summary>
    [DebuggerDisplay("PropertyModel: Name={PropertyName}")]
    public class PropertyModel : ICommonModel, IBindingModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="PropertyModel"/>.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the underlying property.</param>
        /// <param name="attributes">Any attributes which are annotated on the property.</param>
        public PropertyModel(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] IReadOnlyList<object> attributes)
        {
            PropertyInfo = propertyInfo;
            Properties = new Dictionary<object, object>();
            Attributes = new List<object>(attributes);
        }

        /// <summary>
        /// Creats a new instance of <see cref="PropertyModel"/> from a given <see cref="PropertyModel"/>.
        /// </summary>
        /// <param name="other">The <see cref="PropertyModel"/> which needs to be copied.</param>
        public PropertyModel([NotNull] PropertyModel other)
        {
            Controller = other.Controller;
            Attributes = new List<object>(other.Attributes);
            BindingInfo = BindingInfo == null ? null : new BindingInfo(other.BindingInfo);
            PropertyInfo = other.PropertyInfo;
            PropertyName = other.PropertyName;
            Properties = new Dictionary<object, object>(other.Properties);
        }

        /// <summary>
        /// Gets or sets the <see cref="ControllerModel"/> this <see cref="PropertyModel"/> is associated with.
        /// </summary>
        public ControllerModel Controller { get; set; }

        /// <summary>
        /// Gets any attributes which are annotated on the property.
        /// </summary>
        public IReadOnlyList<object> Attributes { get; }

        public IDictionary<object, object> Properties { get; }

        MemberInfo ICommonModel.MemberInfo => PropertyInfo;

        string ICommonModel.Name => PropertyName;

        /// <summary>
        /// Gets or sets the <see cref="BindingInfo"/> associated with this model.
        /// </summary>
        public BindingInfo BindingInfo { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets or sets the name of the property represented by this model.
        /// </summary>
        public string PropertyName { get; set; }
    }
}