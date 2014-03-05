﻿using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.NestedProviders;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Razor;

namespace Microsoft.AspNet.Mvc
{
    public class MvcServices
    {
        public ServiceProvider Services { get; private set; }

        public MvcServices(string appRoot)
            : this(appRoot, null)
        {
        }

        public MvcServices(string appRoot, IServiceProvider hostServiceProvider)
        {
            Services = new ServiceProvider();

            Add<IControllerFactory, DefaultControllerFactory>();
            Add<IControllerDescriptorFactory, DefaultControllerDescriptorFactory>();
            Add<IActionSelector, DefaultActionSelector>();
            Add<IActionInvokerFactory, ActionInvokerFactory>();
            Add<IActionResultHelper, ActionResultHelper>();
            Add<IActionResultFactory, ActionResultFactory>();
            Add<IParameterDescriptorFactory, DefaultParameterDescriptorFactory>();
            Add<IControllerAssemblyProvider, AppDomainControllerAssemblyProvider>();
            Add<IActionDiscoveryConventions, DefaultActionDiscoveryConventions>();
            AddInstance<IFileSystem>(new PhysicalFileSystem(appRoot));
            AddInstance<IMvcRazorHost>(new MvcRazorHost(typeof(RazorView).FullName));

#if NET45
            // TODO: Container chaining to flow services from the host to this container

            Add<ICompilationService, CscBasedCompilationService>();

            // TODO: Make this work like normal when we get container chaining
            // TODO: Update this when we have the new host services
            // AddInstance<ICompilationService>(new RoslynCompilationService(hostServiceProvider));
#endif
            Add<IRazorCompilationService, RazorCompilationService>();
            Add<IVirtualPathViewFactory, VirtualPathViewFactory>();
            Add<IViewEngine, RazorViewEngine>();

            Add<IModelMetadataProvider, DataAnnotationsModelMetadataProvider>();
            Add<IActionBindingContextProvider, DefaultActionBindingContextProvider>();

            // This is temporary until DI has some magic for it
            Add<INestedProviderManager<ActionDescriptorProviderContext>, NestedProviderManager<ActionDescriptorProviderContext>>();
            Add<INestedProviderManager<ActionInvokerProviderContext>, NestedProviderManager<ActionInvokerProviderContext>>();
            Add<INestedProvider<ActionDescriptorProviderContext>, ReflectedActionDescriptorProvider>();
            Add<INestedProvider<ActionInvokerProviderContext>, ActionInvokerProvider>();

            Add<IValueProviderFactory, RouteValueValueProviderFactory>();
            Add<IValueProviderFactory, QueryStringValueProviderFactory>();

            Add<IModelBinder, TypeConverterModelBinder>();
            Add<IModelBinder, TypeMatchModelBinder>();
            Add<IModelBinder, GenericModelBinder>();
            Add<IModelBinder, MutableObjectModelBinder>();
            Add<IModelBinder, ComplexModelDtoModelBinder>();

            Add<IInputFormatter, JsonInputFormatter>();
        }

        private void Add<T, TU>() where TU : T
        {
            Services.Add<T, TU>();
        }

        private void AddInstance<T>(object instance)
        {
            Services.AddInstance<T>(instance);
        }
    }
}
