﻿using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc
{
    public interface IControllerFactory
    {
        object CreateController(HttpContext context, ActionDescriptor actionDescriptor);

        void ReleaseController(object controller);
    }
}
