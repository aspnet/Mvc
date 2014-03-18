// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IView
    {
        Task RenderAsync(ViewContext context, TextWriter writer);
    }
}
