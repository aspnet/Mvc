﻿
using System;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentInvokerFactory
    {
        IViewComponentInvoker CreateInstance([NotNull] Type componentType, [NotNull] object[] args);
    }
}
