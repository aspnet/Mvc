﻿
using System.Collections.Generic;
using Microsoft.AspNet.ConfigurationModel;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.NestedProviders;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class MvcServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            return GetDefaultServices(new Configuration());
        }

        public static IEnumerable<IServiceDescriptor> GetDefaultServices(IConfiguration configuration)
        {
            var describe = new ServiceDescriber(configuration);

            yield return describe.Transient<IControllerFactory, DefaultControllerFactory>();
            yield return describe.Transient<IControllerDescriptorFactory, DefaultControllerDescriptorFactory>();
            yield return describe.Transient<IActionSelector, DefaultActionSelector>();
            yield return describe.Transient<IActionInvokerFactory, ActionInvokerFactory>();
            yield return describe.Transient<IActionResultHelper, ActionResultHelper>();
            yield return describe.Transient<IActionResultFactory, ActionResultFactory>();
            yield return describe.Transient<IParameterDescriptorFactory, DefaultParameterDescriptorFactory>();
            yield return describe.Transient<IControllerAssemblyProvider, DefaultControllerAssemblyProvider>();
            yield return describe.Transient<IActionDiscoveryConventions, DefaultActionDiscoveryConventions>();

            yield return describe.Instance<IMvcRazorHost>(new MvcRazorHost(typeof(RazorView).FullName));

            yield return describe.Transient<ICompilationService, RoslynCompilationService>();

            yield return describe.Transient<IRazorCompilationService, RazorCompilationService>();
            yield return describe.Transient<IVirtualPathViewFactory, VirtualPathViewFactory>();
            yield return describe.Transient<IViewEngine, RazorViewEngine>();

            yield return describe.Transient<INestedProvider<ActionDescriptorProviderContext>,
                                            ReflectedActionDescriptorProvider>();
            yield return describe.Transient<INestedProvider<ActionDescriptorProviderContext>,
                                ReflectedRouteConstraintsActionDescriptorProvider>();
            yield return describe.Transient<INestedProvider<ActionInvokerProviderContext>,
                                            ReflectedActionInvokerProvider>();

            yield return describe.Transient<IModelMetadataProvider, DataAnnotationsModelMetadataProvider>();
            yield return describe.Transient<IActionBindingContextProvider, DefaultActionBindingContextProvider>();

            yield return describe.Transient<IValueProviderFactory, RouteValueValueProviderFactory>();
            yield return describe.Transient<IValueProviderFactory, QueryStringValueProviderFactory>();
            yield return describe.Transient<IValueProviderFactory, FormValueProviderFactory>();

            yield return describe.Transient<IModelBinder, TypeConverterModelBinder>();
            yield return describe.Transient<IModelBinder, TypeMatchModelBinder>();
            yield return describe.Transient<IModelBinder, GenericModelBinder>();
            yield return describe.Transient<IModelBinder, MutableObjectModelBinder>();
            yield return describe.Transient<IModelBinder, ComplexModelDtoModelBinder>();

            yield return describe.Transient<IInputFormatter, JsonInputFormatter>();
            yield return describe.Transient<IInputFormatterProvider, TempInputFormatterProvider>();

            yield return describe.Transient<INestedProvider<FilterProviderContext>, DefaultFilterProvider>();

            yield return describe.Transient<IModelValidatorProvider, DataAnnotationsModelValidatorProvider>();
            yield return describe.Transient<IModelValidatorProvider, DataMemberModelValidatorProvider>();

            yield return
               describe.Describe(
                   typeof(INestedProviderManager<>),
                   typeof(NestedProviderManager<>),
                   implementationInstance: null,
                   lifecycle: LifecycleKind.Transient);

            yield return
                describe.Describe(
                    typeof(INestedProviderManagerAsync<>),
                    typeof(NestedProviderManagerAsync<>),
                    implementationInstance: null,
                    lifecycle: LifecycleKind.Transient);
            yield return describe.Transient<IViewComponentSelector, DefaultViewComponentSelector>();
            yield return describe.Transient<IViewComponentInvokerFactory, DefaultViewComponentInvokerFactory>();
            yield return describe.Transient<INestedProvider<ViewComponentInvokerProviderContext>, DefaultViewComponentInvokerProvider>();
            yield return describe.Transient<IViewComponentResultHelper, DefaultViewComponentResultHelper>();
        }
    }
}
