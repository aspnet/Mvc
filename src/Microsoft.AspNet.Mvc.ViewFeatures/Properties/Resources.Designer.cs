// <auto-generated />
namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.Mvc.ViewFeatures.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// The view component name '{0}' matched multiple types:{1}{2}
        /// </summary>
        internal static string ViewComponent_AmbiguousTypeMatch
        {
            get { return GetString("ViewComponent_AmbiguousTypeMatch"); }
        }

        /// <summary>
        /// The view component name '{0}' matched multiple types:{1}{2}
        /// </summary>
        internal static string FormatViewComponent_AmbiguousTypeMatch(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_AmbiguousTypeMatch"), p0, p1, p2);
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

        /// <summary>
        /// Could not find an '{0}' method matching the parameters.
        /// </summary>
        internal static string ViewComponent_CannotFindMethod
        {
            get { return GetString("ViewComponent_CannotFindMethod"); }
        }

        /// <summary>
        /// Could not find an '{0}' method matching the parameters.
        /// </summary>
        internal static string FormatViewComponent_CannotFindMethod(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_CannotFindMethod"), p0);
        }

        /// <summary>
        /// Could not find an '{0}' or '{1}' method matching the parameters.
        /// </summary>
        internal static string ViewComponent_CannotFindMethod_WithFallback
        {
            get { return GetString("ViewComponent_CannotFindMethod_WithFallback"); }
        }

        /// <summary>
        /// Could not find an '{0}' or '{1}' method matching the parameters.
        /// </summary>
        internal static string FormatViewComponent_CannotFindMethod_WithFallback(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_CannotFindMethod_WithFallback"), p0, p1);
        }

        /// <summary>
        /// View components only support returning {0}, {1} or {2}.
        /// </summary>
        internal static string ViewComponent_InvalidReturnValue
        {
            get { return GetString("ViewComponent_InvalidReturnValue"); }
        }

        /// <summary>
        /// View components only support returning {0}, {1} or {2}.
        /// </summary>
        internal static string FormatViewComponent_InvalidReturnValue(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_InvalidReturnValue"), p0, p1, p2);
        }

        /// <summary>
        /// Value cannot be null or empty.
        /// </summary>
        internal static string ArgumentCannotBeNullOrEmpty
        {
            get { return GetString("ArgumentCannotBeNullOrEmpty"); }
        }

        /// <summary>
        /// Value cannot be null or empty.
        /// </summary>
        internal static string FormatArgumentCannotBeNullOrEmpty()
        {
            return GetString("ArgumentCannotBeNullOrEmpty");
        }

        /// <summary>
        /// The '{0}' property of '{1}' must not be null.
        /// </summary>
        internal static string PropertyOfTypeCannotBeNull
        {
            get { return GetString("PropertyOfTypeCannotBeNull"); }
        }

        /// <summary>
        /// The '{0}' property of '{1}' must not be null.
        /// </summary>
        internal static string FormatPropertyOfTypeCannotBeNull(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("PropertyOfTypeCannotBeNull"), p0, p1);
        }

        /// <summary>
        /// The '{0}' method of type '{1}' cannot return a null value.
        /// </summary>
        internal static string TypeMethodMustReturnNotNullValue
        {
            get { return GetString("TypeMethodMustReturnNotNullValue"); }
        }

        /// <summary>
        /// The '{0}' method of type '{1}' cannot return a null value.
        /// </summary>
        internal static string FormatTypeMethodMustReturnNotNullValue(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("TypeMethodMustReturnNotNullValue"), p0, p1);
        }

        /// <summary>
        /// Property '{0}' is of type '{1}', but this method requires a value of type '{2}'.
        /// </summary>
        internal static string ArgumentPropertyUnexpectedType
        {
            get { return GetString("ArgumentPropertyUnexpectedType"); }
        }

        /// <summary>
        /// Property '{0}' is of type '{1}', but this method requires a value of type '{2}'.
        /// </summary>
        internal static string FormatArgumentPropertyUnexpectedType(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentPropertyUnexpectedType"), p0, p1, p2);
        }

        /// <summary>
        /// The partial view '{0}' was not found or no view engine supports the searched locations. The following locations were searched:{1}
        /// </summary>
        internal static string Common_PartialViewNotFound
        {
            get { return GetString("Common_PartialViewNotFound"); }
        }

        /// <summary>
        /// The partial view '{0}' was not found or no view engine supports the searched locations. The following locations were searched:{1}
        /// </summary>
        internal static string FormatCommon_PartialViewNotFound(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Common_PartialViewNotFound"), p0, p1);
        }

        /// <summary>
        /// False
        /// </summary>
        internal static string Common_TriState_False
        {
            get { return GetString("Common_TriState_False"); }
        }

        /// <summary>
        /// False
        /// </summary>
        internal static string FormatCommon_TriState_False()
        {
            return GetString("Common_TriState_False");
        }

        /// <summary>
        /// Not Set
        /// </summary>
        internal static string Common_TriState_NotSet
        {
            get { return GetString("Common_TriState_NotSet"); }
        }

        /// <summary>
        /// Not Set
        /// </summary>
        internal static string FormatCommon_TriState_NotSet()
        {
            return GetString("Common_TriState_NotSet");
        }

        /// <summary>
        /// True
        /// </summary>
        internal static string Common_TriState_True
        {
            get { return GetString("Common_TriState_True"); }
        }

        /// <summary>
        /// True
        /// </summary>
        internal static string FormatCommon_TriState_True()
        {
            return GetString("Common_TriState_True");
        }

        /// <summary>
        /// ViewData value must not be null.
        /// </summary>
        internal static string DynamicViewData_ViewDataNull
        {
            get { return GetString("DynamicViewData_ViewDataNull"); }
        }

        /// <summary>
        /// ViewData value must not be null.
        /// </summary>
        internal static string FormatDynamicViewData_ViewDataNull()
        {
            return GetString("DynamicViewData_ViewDataNull");
        }

        /// <summary>
        /// The expression compiler was unable to evaluate the indexer expression '{0}' because it references the model parameter '{1}' which is unavailable.
        /// </summary>
        internal static string ExpressionHelper_InvalidIndexerExpression
        {
            get { return GetString("ExpressionHelper_InvalidIndexerExpression"); }
        }

        /// <summary>
        /// The expression compiler was unable to evaluate the indexer expression '{0}' because it references the model parameter '{1}' which is unavailable.
        /// </summary>
        internal static string FormatExpressionHelper_InvalidIndexerExpression(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ExpressionHelper_InvalidIndexerExpression"), p0, p1);
        }

        /// <summary>
        /// The IModelMetadataProvider was unable to provide metadata for expression '{0}'.
        /// </summary>
        internal static string HtmlHelper_NullModelMetadata
        {
            get { return GetString("HtmlHelper_NullModelMetadata"); }
        }

        /// <summary>
        /// The IModelMetadataProvider was unable to provide metadata for expression '{0}'.
        /// </summary>
        internal static string FormatHtmlHelper_NullModelMetadata(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("HtmlHelper_NullModelMetadata"), p0);
        }

        /// <summary>
        /// Must call 'Contextualize' method before using this HtmlHelper instance.
        /// </summary>
        internal static string HtmlHelper_NotContextualized
        {
            get { return GetString("HtmlHelper_NotContextualized"); }
        }

        /// <summary>
        /// Must call 'Contextualize' method before using this HtmlHelper instance.
        /// </summary>
        internal static string FormatHtmlHelper_NotContextualized()
        {
            return GetString("HtmlHelper_NotContextualized");
        }

        /// <summary>
        /// There is no ViewData item of type '{0}' that has the key '{1}'.
        /// </summary>
        internal static string HtmlHelper_MissingSelectData
        {
            get { return GetString("HtmlHelper_MissingSelectData"); }
        }

        /// <summary>
        /// There is no ViewData item of type '{0}' that has the key '{1}'.
        /// </summary>
        internal static string FormatHtmlHelper_MissingSelectData(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("HtmlHelper_MissingSelectData"), p0, p1);
        }

        /// <summary>
        /// The parameter '{0}' must evaluate to an IEnumerable when multiple selection is allowed.
        /// </summary>
        internal static string HtmlHelper_SelectExpressionNotEnumerable
        {
            get { return GetString("HtmlHelper_SelectExpressionNotEnumerable"); }
        }

        /// <summary>
        /// The parameter '{0}' must evaluate to an IEnumerable when multiple selection is allowed.
        /// </summary>
        internal static string FormatHtmlHelper_SelectExpressionNotEnumerable(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("HtmlHelper_SelectExpressionNotEnumerable"), p0);
        }

        /// <summary>
        /// The type '{0}' is not supported. Type must be an {1} that does not have an associated {2}.
        /// </summary>
        internal static string HtmlHelper_TypeNotSupported_ForGetEnumSelectList
        {
            get { return GetString("HtmlHelper_TypeNotSupported_ForGetEnumSelectList"); }
        }

        /// <summary>
        /// The type '{0}' is not supported. Type must be an {1} that does not have an associated {2}.
        /// </summary>
        internal static string FormatHtmlHelper_TypeNotSupported_ForGetEnumSelectList(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("HtmlHelper_TypeNotSupported_ForGetEnumSelectList"), p0, p1, p2);
        }

        /// <summary>
        /// The ViewData item that has the key '{0}' is of type '{1}' but must be of type '{2}'.
        /// </summary>
        internal static string HtmlHelper_WrongSelectDataType
        {
            get { return GetString("HtmlHelper_WrongSelectDataType"); }
        }

        /// <summary>
        /// The ViewData item that has the key '{0}' is of type '{1}' but must be of type '{2}'.
        /// </summary>
        internal static string FormatHtmlHelper_WrongSelectDataType(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("HtmlHelper_WrongSelectDataType"), p0, p1, p2);
        }

        /// <summary>
        /// The '{0}' template was used with an object of type '{1}', which does not implement '{2}'.
        /// </summary>
        internal static string Templates_TypeMustImplementIEnumerable
        {
            get { return GetString("Templates_TypeMustImplementIEnumerable"); }
        }

        /// <summary>
        /// The '{0}' template was used with an object of type '{1}', which does not implement '{2}'.
        /// </summary>
        internal static string FormatTemplates_TypeMustImplementIEnumerable(object p0, object p1, object p2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Templates_TypeMustImplementIEnumerable"), p0, p1, p2);
        }

        /// <summary>
        /// Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.
        /// </summary>
        internal static string TemplateHelpers_TemplateLimitations
        {
            get { return GetString("TemplateHelpers_TemplateLimitations"); }
        }

        /// <summary>
        /// Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.
        /// </summary>
        internal static string FormatTemplateHelpers_TemplateLimitations()
        {
            return GetString("TemplateHelpers_TemplateLimitations");
        }

        /// <summary>
        /// Unable to locate an appropriate template for type {0}.
        /// </summary>
        internal static string TemplateHelpers_NoTemplate
        {
            get { return GetString("TemplateHelpers_NoTemplate"); }
        }

        /// <summary>
        /// Unable to locate an appropriate template for type {0}.
        /// </summary>
        internal static string FormatTemplateHelpers_NoTemplate(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("TemplateHelpers_NoTemplate"), p0);
        }

        /// <summary>
        /// The model item passed is null, but this ViewDataDictionary instance requires a non-null model item of type '{0}'.
        /// </summary>
        internal static string ViewData_ModelCannotBeNull
        {
            get { return GetString("ViewData_ModelCannotBeNull"); }
        }

        /// <summary>
        /// The model item passed is null, but this ViewDataDictionary instance requires a non-null model item of type '{0}'.
        /// </summary>
        internal static string FormatViewData_ModelCannotBeNull(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewData_ModelCannotBeNull"), p0);
        }

        /// <summary>
        /// The model item passed into the ViewDataDictionary is of type '{0}', but this ViewDataDictionary instance requires a model item of type '{1}'.
        /// </summary>
        internal static string ViewData_WrongTModelType
        {
            get { return GetString("ViewData_WrongTModelType"); }
        }

        /// <summary>
        /// The model item passed into the ViewDataDictionary is of type '{0}', but this ViewDataDictionary instance requires a model item of type '{1}'.
        /// </summary>
        internal static string FormatViewData_WrongTModelType(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewData_WrongTModelType"), p0, p1);
        }

        /// <summary>
        /// The partial view '{0}' was not found. The following locations were searched:{1}
        /// </summary>
        internal static string ViewEngine_PartialViewNotFound
        {
            get { return GetString("ViewEngine_PartialViewNotFound"); }
        }

        /// <summary>
        /// The partial view '{0}' was not found. The following locations were searched:{1}
        /// </summary>
        internal static string FormatViewEngine_PartialViewNotFound(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewEngine_PartialViewNotFound"), p0, p1);
        }

        /// <summary>
        /// The view '{0}' was not found. The following locations were searched:{1}.
        /// </summary>
        internal static string ViewEngine_ViewNotFound
        {
            get { return GetString("ViewEngine_ViewNotFound"); }
        }

        /// <summary>
        /// The view '{0}' was not found. The following locations were searched:{1}.
        /// </summary>
        internal static string FormatViewEngine_ViewNotFound(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewEngine_ViewNotFound"), p0, p1);
        }

        /// <summary>
        /// The value must be greater than or equal to zero.
        /// </summary>
        internal static string HtmlHelper_TextAreaParameterOutOfRange
        {
            get { return GetString("HtmlHelper_TextAreaParameterOutOfRange"); }
        }

        /// <summary>
        /// The value must be greater than or equal to zero.
        /// </summary>
        internal static string FormatHtmlHelper_TextAreaParameterOutOfRange()
        {
            return GetString("HtmlHelper_TextAreaParameterOutOfRange");
        }

        /// <summary>
        /// Validation parameter names in unobtrusive client validation rules cannot be empty. Client rule type: {0}
        /// </summary>
        internal static string UnobtrusiveJavascript_ValidationParameterCannotBeEmpty
        {
            get { return GetString("UnobtrusiveJavascript_ValidationParameterCannotBeEmpty"); }
        }

        /// <summary>
        /// Validation parameter names in unobtrusive client validation rules cannot be empty. Client rule type: {0}
        /// </summary>
        internal static string FormatUnobtrusiveJavascript_ValidationParameterCannotBeEmpty(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnobtrusiveJavascript_ValidationParameterCannotBeEmpty"), p0);
        }

        /// <summary>
        /// Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: {0}, client rule type: {1}
        /// </summary>
        internal static string UnobtrusiveJavascript_ValidationParameterMustBeLegal
        {
            get { return GetString("UnobtrusiveJavascript_ValidationParameterMustBeLegal"); }
        }

        /// <summary>
        /// Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: {0}, client rule type: {1}
        /// </summary>
        internal static string FormatUnobtrusiveJavascript_ValidationParameterMustBeLegal(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnobtrusiveJavascript_ValidationParameterMustBeLegal"), p0, p1);
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: {0}
        /// </summary>
        internal static string UnobtrusiveJavascript_ValidationTypeCannotBeEmpty
        {
            get { return GetString("UnobtrusiveJavascript_ValidationTypeCannotBeEmpty"); }
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: {0}
        /// </summary>
        internal static string FormatUnobtrusiveJavascript_ValidationTypeCannotBeEmpty(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnobtrusiveJavascript_ValidationTypeCannotBeEmpty"), p0);
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: "{0}", client rule type: {1}
        /// </summary>
        internal static string UnobtrusiveJavascript_ValidationTypeMustBeLegal
        {
            get { return GetString("UnobtrusiveJavascript_ValidationTypeMustBeLegal"); }
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: "{0}", client rule type: {1}
        /// </summary>
        internal static string FormatUnobtrusiveJavascript_ValidationTypeMustBeLegal(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnobtrusiveJavascript_ValidationTypeMustBeLegal"), p0, p1);
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules must be unique. The following validation type was seen more than once: {0}
        /// </summary>
        internal static string UnobtrusiveJavascript_ValidationTypeMustBeUnique
        {
            get { return GetString("UnobtrusiveJavascript_ValidationTypeMustBeUnique"); }
        }

        /// <summary>
        /// Validation type names in unobtrusive client validation rules must be unique. The following validation type was seen more than once: {0}
        /// </summary>
        internal static string FormatUnobtrusiveJavascript_ValidationTypeMustBeUnique(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("UnobtrusiveJavascript_ValidationTypeMustBeUnique"), p0);
        }

        /// <summary>
        /// The type '{0}' must derive from '{1}'.
        /// </summary>
        internal static string TypeMustDeriveFromType
        {
            get { return GetString("TypeMustDeriveFromType"); }
        }

        /// <summary>
        /// The type '{0}' must derive from '{1}'.
        /// </summary>
        internal static string FormatTypeMustDeriveFromType(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("TypeMustDeriveFromType"), p0, p1);
        }

        /// <summary>
        /// Could not find a replacement for view expansion token '{0}'.
        /// </summary>
        internal static string TemplatedViewLocationExpander_NoReplacementToken
        {
            get { return GetString("TemplatedViewLocationExpander_NoReplacementToken"); }
        }

        /// <summary>
        /// Could not find a replacement for view expansion token '{0}'.
        /// </summary>
        internal static string FormatTemplatedViewLocationExpander_NoReplacementToken(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("TemplatedViewLocationExpander_NoReplacementToken"), p0);
        }

        /// <summary>
        /// {0} must be executed before {1} can be invoked.
        /// </summary>
        internal static string TemplatedExpander_PopulateValuesMustBeInvokedFirst
        {
            get { return GetString("TemplatedExpander_PopulateValuesMustBeInvokedFirst"); }
        }

        /// <summary>
        /// {0} must be executed before {1} can be invoked.
        /// </summary>
        internal static string FormatTemplatedExpander_PopulateValuesMustBeInvokedFirst(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("TemplatedExpander_PopulateValuesMustBeInvokedFirst"), p0, p1);
        }

        /// <summary>
        /// The result of value factory cannot be null.
        /// </summary>
        internal static string TemplatedExpander_ValueFactoryCannotReturnNull
        {
            get { return GetString("TemplatedExpander_ValueFactoryCannotReturnNull"); }
        }

        /// <summary>
        /// The result of value factory cannot be null.
        /// </summary>
        internal static string FormatTemplatedExpander_ValueFactoryCannotReturnNull()
        {
            return GetString("TemplatedExpander_ValueFactoryCannotReturnNull");
        }

        /// <summary>
        /// Type: '{0}' - Name: '{1}'
        /// </summary>
        internal static string ViewComponent_AmbiguousTypeMatch_Item
        {
            get { return GetString("ViewComponent_AmbiguousTypeMatch_Item"); }
        }

        /// <summary>
        /// Type: '{0}' - Name: '{1}'
        /// </summary>
        internal static string FormatViewComponent_AmbiguousTypeMatch_Item(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ViewComponent_AmbiguousTypeMatch_Item"), p0, p1);
        }

        /// <summary>
        /// The property {0}.{1} could not be found.
        /// </summary>
        internal static string Common_PropertyNotFound
        {
            get { return GetString("Common_PropertyNotFound"); }
        }

        /// <summary>
        /// The property {0}.{1} could not be found.
        /// </summary>
        internal static string FormatCommon_PropertyNotFound(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Common_PropertyNotFound"), p0, p1);
        }

        /// <summary>
        /// The value '{0}' is invalid.
        /// </summary>
        internal static string Common_ValueNotValidForProperty
        {
            get { return GetString("Common_ValueNotValidForProperty"); }
        }

        /// <summary>
        /// The value '{0}' is invalid.
        /// </summary>
        internal static string FormatCommon_ValueNotValidForProperty(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Common_ValueNotValidForProperty"), p0);
        }

        /// <summary>
        /// No URL for remote validation could be found.
        /// </summary>
        internal static string RemoteAttribute_NoUrlFound
        {
            get { return GetString("RemoteAttribute_NoUrlFound"); }
        }

        /// <summary>
        /// No URL for remote validation could be found.
        /// </summary>
        internal static string FormatRemoteAttribute_NoUrlFound()
        {
            return GetString("RemoteAttribute_NoUrlFound");
        }

        /// <summary>
        /// '{0}' is invalid.
        /// </summary>
        internal static string RemoteAttribute_RemoteValidationFailed
        {
            get { return GetString("RemoteAttribute_RemoteValidationFailed"); }
        }

        /// <summary>
        /// '{0}' is invalid.
        /// </summary>
        internal static string FormatRemoteAttribute_RemoteValidationFailed(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("RemoteAttribute_RemoteValidationFailed"), p0);
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
