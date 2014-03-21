// <auto-generated />
namespace Microsoft.AspNet.Mvc.Core
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.Mvc.Core.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// The method '{0}' on type '{1}' returned an instance of '{2}'. Make sure to call Unwrap on the returned value to avoid unobserved faulted Task.
        /// </summary>
        internal static string ActionExecutor_WrappedTaskInstance
        {
            get { return GetString("ActionExecutor_WrappedTaskInstance"); }
        }

        /// <summary>
        /// The method '{0}' on type '{1}' returned an instance of '{2}'. Make sure to call Unwrap on the returned value to avoid unobserved faulted Task.
        /// </summary>
        internal static string FormatActionExecutor_WrappedTaskInstance(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ActionExecutor_WrappedTaskInstance"), p0, p1, p2);
        }

        /// <summary>
        /// The method '{0}' on type '{1}' returned a Task instance even though it is not an asynchronous method.
        /// </summary>
        internal static string ActionExecutor_UnexpectedTaskInstance
        {
            get { return GetString("ActionExecutor_UnexpectedTaskInstance"); }
        }

        /// <summary>
        /// The method '{0}' on type '{1}' returned a Task instance even though it is not an asynchronous method.
        /// </summary>
        internal static string FormatActionExecutor_UnexpectedTaskInstance(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ActionExecutor_UnexpectedTaskInstance"), p0, p1);
        }

        /// <summary>
        /// The class ReflectedActionFilterEndPoint only supports ReflectedActionDescriptors.
        /// </summary>
        internal static string ReflectedActionFilterEndPoint_UnexpectedActionDescriptor
        {
            get { return GetString("ReflectedActionFilterEndPoint_UnexpectedActionDescriptor"); }
        }

        /// <summary>
        /// The class ReflectedActionFilterEndPoint only supports ReflectedActionDescriptors.
        /// </summary>
        internal static string FormatReflectedActionFilterEndPoint_UnexpectedActionDescriptor()
        {
            return GetString("ReflectedActionFilterEndPoint_UnexpectedActionDescriptor");
        }

        /// <summary>
        /// The view component name '{0}' matched multiple types: {1}
        /// </summary>
        internal static string ViewComponent_AmbiguousTypeMatch
        {
            get { return GetString("ViewComponent_AmbiguousTypeMatch"); }
        }

        /// <summary>
        /// The view component name '{0}' matched multiple types: {1}
        /// </summary>
        internal static string FormatViewComponent_AmbiguousTypeMatch(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_AmbiguousTypeMatch"), p0, p1);
        }

        /// <summary>
        /// The async view component method '{0}' should be declared to return Task&lt;T&gt;.
        /// </summary>
        internal static string ViewComponent_AsyncMethod_ShouldReturnTask
        {
            get { return GetString("ViewComponent_AsyncMethod_ShouldReturnTask"); }
        }

        /// <summary>
        /// The async view component method '{0}' should be declared to return Task&lt;T&gt;.
        /// </summary>
        internal static string FormatViewComponent_AsyncMethod_ShouldReturnTask(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_AsyncMethod_ShouldReturnTask"), p0);
        }

        /// <summary>
        /// A view component must return a non-null value.
        /// </summary>
        internal static string ViewComponent_MustReturnValue
        {
            get { return GetString("ViewComponent_MustReturnValue"); }
        }

        /// <summary>
        /// A view component must return a non-null value.
        /// </summary>
        internal static string FormatViewComponent_MustReturnValue()
        {
            return GetString("ViewComponent_MustReturnValue");
        }

        /// <summary>
        /// The view component method '{0}' should be declared to return a value.
        /// </summary>
        internal static string ViewComponent_SyncMethod_ShouldReturnValue
        {
            get { return GetString("ViewComponent_SyncMethod_ShouldReturnValue"); }
        }

        /// <summary>
        /// The view component method '{0}' should be declared to return a value.
        /// </summary>
        internal static string FormatViewComponent_SyncMethod_ShouldReturnValue(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_SyncMethod_ShouldReturnValue"), p0);
        }

        /// <summary>
        /// A view component named '{0}' could not be found.
        /// </summary>
        internal static string ViewComponent_CannotFindComponent
        {
            get { return GetString("ViewComponent_CannotFindComponent"); }
        }

        /// <summary>
        /// A view component named '{0}' could not be found.
        /// </summary>
        internal static string FormatViewComponent_CannotFindComponent(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_CannotFindComponent"), p0);
        }

        /// <summary>
        /// An invoker could not be created for the view component '{0}'.
        /// </summary>
        internal static string ViewComponent_IViewComponentFactory_ReturnedNull
        {
            get { return GetString("ViewComponent_IViewComponentFactory_ReturnedNull"); }
        }

        /// <summary>
        /// An invoker could not be created for the view component '{0}'.
        /// </summary>
        internal static string FormatViewComponent_IViewComponentFactory_ReturnedNull(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_IViewComponentFactory_ReturnedNull"), p0);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);
    
            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
