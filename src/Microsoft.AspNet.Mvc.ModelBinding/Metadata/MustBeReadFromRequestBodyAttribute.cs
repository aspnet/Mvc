using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Attribute on a parameter or type that declares that the target will consume the entire body. 
    /// If the attribute is on a type-declaration, then it's as if that attribute is present on all action parameters 
    /// of that type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class MustBeReadFromRequestBodyAttribute : Attribute
    {
    }
}
