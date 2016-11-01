// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal;
using Microsoft.Extensions.Logging;
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
    public class XmlBodylModelBindersTest
    {
        [Theory]
        [InlineData(XmlSerializerType.XmlSeriralizer, false)]
        [InlineData(XmlSerializerType.XmlSeriralizer, true)]
        [InlineData(XmlSerializerType.DataContractSerializer, false)]
        [InlineData(XmlSerializerType.DataContractSerializer, true)]
        public async void BindTheXmlBodyToTheParameterValue(XmlSerializerType xmlSerializerType, bool useXmlBinderOnly)
        {
            // Arrange 
            byte[] bodyRequestContext = new byte[0];

            var value = new PurchaseOrder();
            var xmlWriterSettings = FormattingUtilities.GetDefaultXmlWriterSettings();
            xmlWriterSettings.CloseOutput = false;
            var textw = new StringWriter();
            var writer = XmlWriter.Create(textw, xmlWriterSettings);
            if (xmlSerializerType == XmlSerializerType.XmlSeriralizer)
            {
                var xmlSerializer = new XmlSerializer(value.GetType());
                xmlSerializer.Serialize(writer, value);
                bodyRequestContext = Encoding.UTF8.GetBytes(textw.ToString());
            }
            else
            {
                var xmlSerializer = new DataContractSerializer(value.GetType());
                xmlSerializer.WriteObject(writer, value);
                writer.Flush();
                bodyRequestContext = Encoding.UTF8.GetBytes(textw.ToString());
            }

            var att = new FromXmlBodyAttribute()
            {
                XmlSerializerType = xmlSerializerType,
                UseXmlBinderOnly = useXmlBinderOnly
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

            actionContext.HttpContext.Request.Body.Write(bodyRequestContext, 0, bodyRequestContext.Length);
            actionContext.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

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

            // Act
            var binderforType = binderTypeModelBinderProvider.GetBinder(modelBinderProviderContext);

            // Assert
            Assert.NotNull(binderforType);
            await binderforType.BindModelAsync(modelBindingContext);

            var newValue = modelBindingContext.Result.Model as PurchaseOrder;
            Assert.NotNull(newValue);
            Assert.Equal(value.billTo.street, newValue.billTo.street);
            Assert.Equal(value.shipTo.street, newValue.shipTo.street);

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
            IHttpRequestStreamReaderFactory readerFactory = new TestHttpRequestStreamReaderFactory();
            ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
            var services = new ServiceCollection();

            services.AddOptions();

            services.AddSingleton(readerFactory);
            services.AddSingleton(writerFactory);
            services.AddSingleton(loggerFactory);

            services.TryAddTransient<DcXmlBodyModelBinder>();
            services.TryAddTransient<DcXmlBodyModelBinderOnly>();

            services.TryAddTransient<XmlBodyModelBinder>();
            services.TryAddTransient<XmlBodyModelBinderOnly>();

            return services;
        }

        private static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Body = new MemoryStream();
            httpContext.Request.ContentType = "application/xml";
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
