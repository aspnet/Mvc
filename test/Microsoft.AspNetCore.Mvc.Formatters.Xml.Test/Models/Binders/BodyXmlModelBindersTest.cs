// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Test.Models;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http.Internal;
using System.Globalization;
using Microsoft.Extensions.Primitives;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Test.Models.Binders
{
    public class BodyXmlModelBindersTest
    {
        [Fact]
        public void BindTheXmlToTheValue()
        {

            var att = new FromBodyXmlAttribute()
            {
                XmlSerializerType = XmlSerializerType.XmlSeriralizer,
                UseXmlBinderOnly = false
            };
            var attList = new List<object>() { att };
            var bindingInfo = BindingInfo.GetBindingInfo(attList);

            var parameterDescriptor = new ParameterDescriptor
            {
                Name = "value",
                ParameterType = typeof(PurchaseOrder),
                BindingInfo = bindingInfo
            };

            var actionDescriptor = new ActionDescriptor()
            {
                Parameters = new List<ParameterDescriptor>() { parameterDescriptor }
            };

            var actionContext = GetActionContext(actionDescriptor);
            //TODO: Add service 
            ServiceCollection services = CreateServices();


            var servicesProvider = services.BuildServiceProvider();
            actionContext.HttpContext.RequestServices = servicesProvider;

            var metadataProvider = new TestModelMetadataProvider();

            metadataProvider.ForType(parameterDescriptor.ParameterType).BindingDetails
                (
                (b) =>
                    {
                        b.BindingSource = BindingSource.Body;
                        b.BinderType = att.BinderType;
                    }
            );

            ModelMetadata parameterModelMetadata = metadataProvider.GetMetadataForType(parameterDescriptor.ParameterType);

            var original = CreateDefaultValueProvider();

            //*1
            ModelBindingContext modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
            actionContext,
            original,
            parameterModelMetadata,
            parameterDescriptor.BindingInfo,
            "model");

            //*2
            ModelBinderProviderContext modelBinderProviderContext = new TestModelBinderProviderContext(parameterModelMetadata, parameterDescriptor.BindingInfo, metadataProvider);

            BinderTypeModelBinderProvider binderTypeModelBinderProvider = new BinderTypeModelBinderProvider();
            
           var binderforType =  binderTypeModelBinderProvider.GetBinder(modelBinderProviderContext);

        }

        private static CompositeValueProvider CreateDefaultValueProvider()
        {
            var result = new CompositeValueProvider();
            result.Add(new RouteValueProvider(BindingSource.Path, new RouteValueDictionary()));
            result.Add(new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(),
                CultureInfo.InvariantCulture));
            result.Add(new FormValueProvider(
                BindingSource.Form,
                new FormCollection(new Dictionary<string, StringValues>()),
                CultureInfo.CurrentCulture));
            return result;
        }

        private static ServiceCollection CreateServices()
        {
            IHttpResponseStreamWriterFactory writerFactory = new TestHttpResponseStreamWriterFactory();
            ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
            var services = new ServiceCollection();

            services.AddOptions();

            services.AddSingleton(writerFactory);
            services.AddSingleton(loggerFactory);

            services.TryAddTransient<BodyDcXmlModelBinder>();
            services.TryAddTransient<BodyDcXmlModelBinderOnly>();

            services.TryAddTransient<BodyXmlModelBinder>();
            services.TryAddTransient<BodyXmlModelBinderOnly>();
                        
            return services;
        }

        private static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            return httpContext;
        }


        private static ActionContext GetActionContext(ActionDescriptor actionDescriptor)
        {
            return new ActionContext(GetHttpContext(), new RouteData(), actionDescriptor);
        }

        private static byte[] GetWrittenBytes(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return Assert.IsType<MemoryStream>(context.Response.Body).ToArray();
        }
    }

    internal class TestModelBinderProviderContext : ModelBinderProviderContext
    {
        private BindingInfo bindingInfo;
        private ModelMetadata parameterModelMetadata;
        private IModelMetadataProvider modelMetadataProvider;

        public TestModelBinderProviderContext(ModelMetadata parameterModelMetadata, BindingInfo bindingInfo, IModelMetadataProvider modelMetadataProvider)
        {
            this.parameterModelMetadata = parameterModelMetadata;
            this.bindingInfo = bindingInfo;
            this.modelMetadataProvider = modelMetadataProvider;
        }

        public override BindingInfo BindingInfo
        {
            get
            {
                return bindingInfo;
            }
        }

        public override ModelMetadata Metadata
        {
            get
            {
                return parameterModelMetadata;
            }
        }

        public override IModelMetadataProvider MetadataProvider
        {
            get
            {
                return modelMetadataProvider;
            }
        }

        public override IModelBinder CreateBinder(ModelMetadata metadata)
        {
            throw new NotImplementedException();
        }
    }
}
