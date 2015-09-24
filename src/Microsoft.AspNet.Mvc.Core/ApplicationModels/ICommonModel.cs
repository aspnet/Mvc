using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface ICommonModel : IPropertyModel
    {
        IReadOnlyList<object> Attributes { get; }
        MemberInfo MemberInfo { get; }
        string Name { get; }
    }
}
