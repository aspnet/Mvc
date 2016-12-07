// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc
{
    public class TestOptions<TOptions> : IOptions<TOptions>
        where TOptions : class, new()
    {
        public TOptions Value { get; set; } = new TOptions();
    }
}
