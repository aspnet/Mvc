// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Performance
{
    public class ModelBindingObjectConstructionBenchmark
    {
        private const int ModelCount = 20;
        private const string ModelPrefix = "models";

        private ParameterBinder _binder;

        private ActionContext _actionContext;
        private ParameterDescriptor _parameter;
        private IModelBinder _modelBinder;
        private IValueProvider _valueProvider;
        private ModelMetadata _modelMetadata;

        [GlobalSetup]
        public void Setup()
        {
            var services = GetServices();

            _binder = ActivatorUtilities.CreateInstance<ParameterBinder>(services);

            _parameter = new ParameterDescriptor()
            {
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = ModelPrefix,
                },
                Name = ModelPrefix,
                ParameterType = typeof(MyModel[]),
            };

            var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();
            _modelMetadata = metadataProvider.GetMetadataForType(typeof(MyModel[]));

            _actionContext = new ActionContext();

            var binderFactory = services.GetRequiredService<IModelBinderFactory>();
            _modelBinder = binderFactory.CreateBinder(new ModelBinderFactoryContext()
            {
                BindingInfo = _parameter.BindingInfo,
                CacheToken = _parameter,
                Metadata = _modelMetadata,
            });

            var formCollection = new FormCollection(GetValues(ModelCount));
            _valueProvider = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);
        }

        [Benchmark]
        public async Task<ModelBindingResult> CreateObjects()
        {
            return await _binder.BindModelAsync(
                _actionContext,
                _modelBinder,
                _valueProvider,
                _parameter,
                _modelMetadata,
                value: null);
        }

        private static IServiceProvider GetServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            serviceCollection.AddMvcCore();
            return serviceCollection.BuildServiceProvider();
        }

        private static Dictionary<string, StringValues> GetValues(int count)
        {
            var values = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < count; i++)
            {
                foreach (var property in typeof(MyModel).GetProperties())
                {
                    values[$"{ModelPrefix}[{i}].{property.Name}"] = Guid.NewGuid().ToString();
                }
            }

            return values;
        }

        public class MyModel
        {
            public string Prop0 { get; set; }
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
            public string Prop4 { get; set; }
            public string Prop5 { get; set; }
            public string Prop6 { get; set; }
            public string Prop7 { get; set; }
            public string Prop8 { get; set; }
            public string Prop9 { get; set; }
        }
    }
}
