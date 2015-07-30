// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Formatters.Xml;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class MvcXmlDataContractSerializerMvcOptionsSetup : ConfigureOptions<MvcOptions>
    {
        public MvcXmlDataContractSerializerMvcOptionsSetup()
            : base(ConfigureMvc)
        {
            Order = DefaultOrder.DefaultFrameworkSortOrder + 10;
        }

        public static void ConfigureMvc(MvcOptions options)
        {
            options.ModelMetadataDetailsProviders.Add(new DataMemberRequiredBindingMetadataProvider());

            options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            options.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

            options.ValidationExcludeFilters.Add(typeFullName: "System.Xml.Linq.XObject");
            options.ValidationExcludeFilters.Add(typeFullName: "System.Xml.XmlNode");
        }
    }
}
