// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Formatters.Xml;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class MvcXmlSerializerMvcOptionsSetup : ConfigureOptions<MvcOptions>
    {
        public MvcXmlSerializerMvcOptionsSetup()
            : base(ConfigureMvc)
        {
            Order = DefaultOrder.DefaultFrameworkSortOrder + 10;
        }

        public static void ConfigureMvc(MvcOptions options)
        {
            options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            options.InputFormatters.Add(new XmlSerializerInputFormatter());
        }
    }
}
