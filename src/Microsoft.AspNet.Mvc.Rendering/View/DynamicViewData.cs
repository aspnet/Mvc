using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class DynamicViewData : DynamicObject
    {
        private readonly Func<ViewData> _viewDataFunc;

        public DynamicViewData([NotNull] Func<ViewData> viewDataFunc)
        {
            _viewDataFunc = viewDataFunc;
        }

        private ViewData ViewData
        {
            get
            {
                ViewData viewData = _viewDataFunc();
                if (viewData == null)
                {
                    throw new InvalidOperationException(Resources.DynamicViewData_ViewDataNull);
                }

                return viewData;
            }
        }

        // Implementing this function improves the debugging experience as it provides the debugger with the list of
        // all properties currently defined on the object. In addition code such as that in Json.Net can better-handle
        // (correctly serialize in that case) a DynamicObject that overrides this method.
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ViewData.Keys;
        }

        public override bool TryGetMember([NotNull] GetMemberBinder binder, out object result)
        {
            result = ViewData[binder.Name];

            // ViewData[key] will never throw a KeyNotFoundException. Similarly, return true so caller does not throw.
            return true;
        }

        public override bool TrySetMember([NotNull] SetMemberBinder binder, object value)
        {
            ViewData[binder.Name] = value;

            // Can always add / update a ViewData value.
            return true;
        }
    }
}
