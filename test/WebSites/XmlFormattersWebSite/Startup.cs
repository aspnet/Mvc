﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Xml;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace XmlFormattersWebSite
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var configuration = app.GetTestConfiguration();

            // Set up application services
            app.UseServices(services =>
            {
                // Add MVC services to the services container
                services.AddMvc(configuration);

                services.Configure<MvcOptions>(options =>
                    {
                        options.InputFormatters.Clear();
                        options.OutputFormatters.Clear();

                        // Since both XmlSerializer and DataContractSerializer based formatters
                        // have supported media types of 'application/xml' and 'text/xml',  it 
                        // would be difficult for a test to choose a particular formatter based on
                        // request information (Ex: Accept header).
                        // So here we instead clear out the default supported media types and create new
                        // ones which are distinguishable between formatters.
                        var xmlSerializerInputFormatter = new XmlSerializerInputFormatter();
                        xmlSerializerInputFormatter.SupportedMediaTypes.Clear();
                        xmlSerializerInputFormatter.SupportedMediaTypes.Add(
                            new MediaTypeHeaderValue("application/xml-xmlser"));
                        xmlSerializerInputFormatter.SupportedMediaTypes.Add(
                            new MediaTypeHeaderValue("text/xml-xmlser"));

                        var xmlSerializerOutputFormatter = new XmlSerializerOutputFormatter();
                        xmlSerializerOutputFormatter.SupportedMediaTypes.Clear();
                        xmlSerializerOutputFormatter.SupportedMediaTypes.Add(
                            new MediaTypeHeaderValue("application/xml-xmlser"));
                        xmlSerializerOutputFormatter.SupportedMediaTypes.Add(
                            new MediaTypeHeaderValue("text/xml-xmlser"));

                        var dcsInputFormatter = new XmlDataContractSerializerInputFormatter();
                        dcsInputFormatter.SupportedMediaTypes.Clear();
                        dcsInputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml-dcs"));
                        dcsInputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml-dcs"));

                        var dcsOutputFormatter = new XmlDataContractSerializerOutputFormatter();
                        dcsOutputFormatter.SupportedMediaTypes.Clear();
                        dcsOutputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml-dcs"));
                        dcsOutputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml-dcs"));

                        options.InputFormatters.Add(dcsInputFormatter);
                        options.InputFormatters.Add(xmlSerializerInputFormatter);
                        options.OutputFormatters.Add(dcsOutputFormatter);
                        options.OutputFormatters.Add(xmlSerializerOutputFormatter);
                    });
            });

            app.UseErrorReporter();

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute("ActionAsMethod", "{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });

            });
        }
    }
}
