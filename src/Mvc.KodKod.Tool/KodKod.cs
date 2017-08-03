using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Mvc.KodKod.Tool
{
    class KodKod
    {
        public string OutputPath { get; set; }

        public string ApplicationName { get; set; }

        public string BaseUrl { get; set; }

        public void Execute()
        {
            var assemblyName = new AssemblyName(ApplicationName);
            var assembly = Assembly.Load(assemblyName);
            var serviceProvider = GetServiceProvider(assembly);

            var apiDescriptorProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var apiDescriptions = apiDescriptorProvider.ApiDescriptionGroups.Items;

            var document = new SwaggerDocument
            {
                SwaggerVersion = "1.2",
                BasePath = BaseUrl,
            };

            foreach (var description in apiDescriptions.SelectMany(d => d.Items))
            {
                var operation = new SwaggerOperation();
                foreach (var parameter in description.ParameterDescriptions)
                {
                    operation.Parameters.Add(new SwaggerParameter
                    {
                        Name = parameter.Name,
                        Type = parameter.ParameterDescriptor.ParameterType.Name,
                        Required = parameter.ModelMetadata.IsBindingRequired,
                    });
                }

                var api = new SwaggerApi
                {
                    Path = description.RelativePath,
                    Operations =
                    {
                        operation,
                    }
                };

                document.Apis.Add(api);
            }

            File.WriteAllText(OutputPath, JsonConvert.SerializeObject(document));
        }

        private IServiceProvider GetServiceProvider(Assembly applicationAssembly)
        {
            var services = new ServiceCollection();

            var hostingEnvironment = new HostingEnvironment
            {
                ApplicationName = ApplicationName,
            };
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore.Mvc.KodKod");

            services
                .AddSingleton<IHostingEnvironment>(hostingEnvironment)
                .AddSingleton<DiagnosticSource>(diagnosticSource)
                .AddLogging()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var mvcBuilder = services.AddMvc();

            var configureType = applicationAssembly
                .GetExportedTypes()
                .FirstOrDefault(typeof(IDesignTimeMvcBuilderConfiguration).IsAssignableFrom);

            if (configureType != null)
            {
                var configureInstance = (IDesignTimeMvcBuilderConfiguration)Activator.CreateInstance(configureType);
                configureInstance.ConfigureMvc(mvcBuilder);
            }

            return mvcBuilder.Services.BuildServiceProvider();
        }
    }
}
