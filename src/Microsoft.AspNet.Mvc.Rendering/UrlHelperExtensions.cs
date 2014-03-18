// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class UrlHelperExtensions
    {
        public static string Action([NotNull] this IUrlHelper generator)
        {
            return generator.Action(action: null, controller: null, values: null);
        }

        public static string Action([NotNull] this IUrlHelper generator, string action)
        {
            return generator.Action(action: action, controller: null, values: null);
        }

        public static string Action([NotNull] this IUrlHelper generator, string action, object values)
        {
            return generator.Action(action: action, controller: null, values: values);
        }

        public static string Action([NotNull] this IUrlHelper generator, string action, string controller)
        {
            return generator.Action(action: action, controller: controller, values: null);
        }
    }
}
