// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    /// <summary>
    /// Provides the currently cached collection of <see cref="Abstractions.ActionDescriptor"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default implementation internally caches the collection and uses
    /// <see cref="IActionDescriptorChangeProvider"/> to invalidate this cache, incrementing
    /// <see cref="ActionDescriptorCollection.Version"/> the collection is reconstructed.
    ///</para>
    ///<para>
    /// To be reactively notified of changes, downcast to <see cref="ActionDescriptorCollectionProvider"/> and
    /// subscribe to the change token returned from <see cref="ActionDescriptorCollectionProvider.GetChangeToken"/>
    /// using <see cref="ChangeToken.OnChange(System.Func{IChangeToken}, System.Action)"/>.
    /// </para>
    /// <para>
    /// Default consumers of this service, are aware of the version and will recache
    /// data as appropriate, but rely on the version being unique.
    /// </para>
    /// </remarks>
    public interface IActionDescriptorCollectionProvider
    {
        /// <summary>
        /// Returns the current cached <see cref="ActionDescriptorCollection"/>
        /// </summary>
        ActionDescriptorCollection ActionDescriptors { get; }
    }
}