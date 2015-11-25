// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Html.Abstractions;

namespace Microsoft.AspNet.Mvc.Razor
{
    public delegate Task RenderAsyncDelegate(IHtmlContentBuilder writer);
}