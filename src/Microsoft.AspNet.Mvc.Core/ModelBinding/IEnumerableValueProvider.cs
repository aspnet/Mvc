// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IEnumerableValueProvider : IValueProvider
    {
        Task<IDictionary<string, string>> GetKeysFromPrefixAsync(string prefix);
    }
}
