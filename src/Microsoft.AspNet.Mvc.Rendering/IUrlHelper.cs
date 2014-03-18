// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IUrlHelper
    {
        string Action(string action, string controller, object values);

        string Route(object values);
    }
}
