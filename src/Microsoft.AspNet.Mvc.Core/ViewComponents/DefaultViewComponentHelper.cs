
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentHelper : IViewComponentHelper
    {
        private readonly TextWriter _body;
        private readonly IViewComponentInvokerFactory _invokerFactory;
        private readonly IViewComponentSelector _selector;
        private readonly ViewContext _viewContext;

        public DefaultViewComponentHelper(
            [NotNull] IViewComponentSelector selector,
            [NotNull] IViewComponentInvokerFactory invokerFactory,
            [NotNull] ViewContext viewContext,
            [NotNull] TextWriter body)
        {
            _selector = selector;
            _invokerFactory = invokerFactory;
            _viewContext = viewContext;
            _body = body;
        }

        public HtmlString Invoke([NotNull] string name, params object[] args)
        {
            var componentType = SelectComponent(name);

            using (var writer = new StringWriter())
            {
                InvokeCore(writer, componentType, args);
                return new HtmlString(writer.ToString());
            }
        }

        public HtmlString Invoke([NotNull] Type componentType, params object[] args)
        {
            using (var writer = new StringWriter())
            {
                InvokeCore(writer, componentType, args);
                return new HtmlString(writer.ToString());
            }
        }

        public void RenderInvoke([NotNull] string name, params object[] args)
        {
            var componentType = SelectComponent(name);

            InvokeCore(_body, componentType, args);
        }

        public void RenderInvoke([NotNull] Type componentType, params object[] args)
        {
            InvokeCore(_body, componentType, args);
        }

        public async Task<HtmlString> InvokeAsync([NotNull] string name, params object[] args)
        {
            var componentType = SelectComponent(name);

            using (var writer = new StringWriter())
            {
                await InvokeCoreAsync(writer, componentType, args);
                return new HtmlString(writer.ToString());
            }
        }

        public async Task<HtmlString> InvokeAsync([NotNull] Type componentType, params object[] args)
        {
            using (var writer = new StringWriter())
            {
                await InvokeCoreAsync(writer, componentType, args);
                return new HtmlString(writer.ToString());
            }
        }

        public async Task RenderInvokeAsync([NotNull] string name, params object[] args)
        {
            var componentType = SelectComponent(name);
            await InvokeCoreAsync(_body, componentType, args);
        }

        public async Task RenderInvokeAsync([NotNull] Type componentType, params object[] args)
        {
            await InvokeCoreAsync(_body, componentType, args);
        }

        private Type SelectComponent([NotNull] string name)
        {
            var componentType = _selector.SelectComponent(name);
            if (componentType == null)
            {
                throw new InvalidOperationException(Resources.FormatViewComponent_CannotFindComponent(name));
            }

            return componentType;
        }

        private async Task InvokeCoreAsync([NotNull] TextWriter writer, [NotNull] Type componentType, object[] arguments)
        {
            var invoker = _invokerFactory.CreateInstance(componentType, arguments);
            if (invoker == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatViewComponent_IViewComponentFactory_ReturnedNull(componentType));
            }

            var context = new ComponentContext(componentType.GetTypeInfo(), _viewContext, writer);
            await invoker.InvokeAsync(context);
        }

        private void InvokeCore([NotNull] TextWriter writer, [NotNull] Type componentType, object[] arguments)
        {
            var invoker = _invokerFactory.CreateInstance(componentType, arguments);
            if (invoker == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatViewComponent_IViewComponentFactory_ReturnedNull(componentType));
            }

            var context = new ComponentContext(componentType.GetTypeInfo(), _viewContext, writer);
            invoker.Invoke(context);
        }
    }
}
