using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.PipelineCore;
using Microsoft.Net.Runtime;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class MvcTestServer
    {
        private RequestDelegate _requestDelegate;
        public MvcTestServer([NotNull] RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task<HttpResponse> SendRequest(HttpRequest request)
        {
            HttpContext context = request.HttpContext;
            await _requestDelegate(context);
            context.Response.Body.Position = 0;
            return context.Response;
        }

        public static MvcTestServer Create<T>() where T : class, new()
        {
            IApplicationEnvironment environment = new TestApplicationEnvironment();
            return Create<T>(environment);
        }

        public static MvcTestServer Create<T>(IApplicationEnvironment environment)
            where T : class, new()
        {
            var collection = new ServiceCollection();
            collection.Add(GetDefaultTestServices(environment));
            var provider = collection.BuildServiceProvider();
            return Create<T>(provider);
        }

        public static MvcTestServer Create<T>(IServiceProvider defaultProvider)
            where T : class, new()
        {
            var builder = new Builder(defaultProvider);

            CallStartup<T>(builder);

            var requestDelegate = builder.Build();
            return new MvcTestServer(requestDelegate);
        }

        private static void CallStartup<T>(Builder builder)
        {
            // We look for two methods on the Startup class, CreateServices that return the collection
            // of configured services that we use to create the IServiceDescriptor (After we added mocked instances for
            // some services) and a ConfigurationCore method that we use to build the pipeline. The purpose of this
            // pattern is to be used in samples, where we want to do in memory testing of an existing app. A user can
            // refactor it's configuration method like follows:
            // var services = CreateServices();
            // var serviceProvider = services.BuildServiceProvider(builder.ServiceProvider);
            // ConfigurationCore(builder, serviceProvider);
            // This is also a very common way of structuring the application services in order to test the IoC
            // dependency resolution chain.
            if (!CreateServicesPattern(builder, typeof(T)))
            {
                ConfigurationPattern(builder, typeof(T));
            }
        }

        private static void ConfigurationPattern([NotNull] IBuilder builder, [NotNull] Type type)
        {
            var flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

            var configurationMethod = type.GetMethods(flags)
                                           .Where(m => m.Name == "Configuration" &&
                                                       m.ReturnType == typeof(void) &&
                                                       m.GetParameters().Length == 1 &&
                                                       m.GetParameters()[0].ParameterType == typeof(IBuilder))
                                           .SingleOrDefault();

            if (configurationMethod == null)
            {
                var errorMessage = string.Format("The type '{0}' must have a single Configuration method", type.FullName);
                throw new InvalidOperationException(errorMessage);
            }

            object instance = null;
            if (!configurationMethod.IsStatic)
            {
                instance = Activator.CreateInstance(type);
            }
            configurationMethod.Invoke(instance, new object[] { builder });
        }

        private static bool CreateServicesPattern([NotNull] IBuilder builder, [NotNull] Type type)
        {
            var flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var createServiceMethods = type.GetMethods(flags)
                              .Where(m => m.Name == "CreateServices" && m.ReturnType == typeof(ServiceCollection));
            if (createServiceMethods.Count() != 1)
            {
                return false;
            }
            var createServices = createServiceMethods.Single();

            var configurationCoreMethods = type.GetMethods(flags)
                                               .Where(m => m.Name == "ConfigurationCore" &&
                                                           m.ReturnType == typeof(void) &&
                                                           m.GetParameters().Length == 2 &&
                                                           m.GetParameters()[0].ParameterType == typeof(IBuilder) &&
                                                           m.GetParameters()[1].ParameterType == typeof(IServiceProvider));
            if (configurationCoreMethods.Count() != 1)
            {
                return false;
            }
            var configurationCore = configurationCoreMethods.Single();

            if (createServices.IsStatic != configurationCore.IsStatic)
            {
                return false;
            }

            object instance = null;
            if (!createServices.IsStatic)
            {
                instance = Activator.CreateInstance(type);
            }
            var collection = (ServiceCollection)createServices.Invoke(instance, new object[0]);
            var classAssemblyProviderType = typeof(ClassAssemblyControllerProvider<>).MakeGenericType(type);
            collection.Add(new ServiceDescriptor
            {
                ServiceType = typeof(IControllerAssemblyProvider),
                ImplementationType = classAssemblyProviderType,
                Lifecycle = LifecycleKind.Transient
            });

            var serviceProvider = collection.BuildServiceProvider(builder.ServiceProvider);
            configurationCore.Invoke(instance, new object[] { builder, serviceProvider });
            return true;
        }

        private static IEnumerable<IServiceDescriptor> GetDefaultTestServices(IApplicationEnvironment environment)
        {
            var describer = new ServiceDescriber();
            yield return describer.Instance<IApplicationEnvironment>(environment);
            yield return describer.Transient<ITypeActivator, TypeActivator>();

            yield return new ServiceDescriptor
            {
                ServiceType = typeof(IContextAccessor<>),
                ImplementationType = typeof(ContextAccessor<>),
                Lifecycle = LifecycleKind.Scoped
            };
        }
    }
}
