using Microsoft.Framework.DependencyInjection;
using System;

namespace Microsoft.AspNet.Mvc
{
    internal static class ActivatorUtilitiesHelper
    {
        public static Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
    }
}