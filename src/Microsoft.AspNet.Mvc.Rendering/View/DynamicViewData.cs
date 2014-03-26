﻿using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class DynamicViewData : DynamicObject
    {
        private readonly Func<ViewDataDictionary> _viewDataFunc;

        public DynamicViewData([NotNull] Func<ViewDataDictionary> viewDataFunc)
        {
            _viewDataFunc = viewDataFunc;
        }

        private ViewDataDictionary ViewData
        {
            get
            {
                ViewDataDictionary viewData = _viewDataFunc();
                if (viewData == null)
                {
                    throw new InvalidOperationException(Resources.DynamicViewData_ViewDataNull);
                }

                return viewData;
            }
        }

        // Implementing this function extends the ViewBag contract, supporting or improving some scenarios. For example
        // having this method improves the debugging experience as it provides the debugger with the list of all
        // properties currently defined on the object.
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ViewData.Keys;
        }

        public override bool TryGetMember([NotNull] GetMemberBinder binder, out object result)
        {
            result = ViewData[binder.Name];

            // ViewDataDictionary[key] will never throw a KeyNotFoundException.
            // Similarly, return true so caller does not throw.
            return true;
        }

        public override bool TrySetMember([NotNull] SetMemberBinder binder, object value)
        {
            ViewData[binder.Name] = value;

            // Can always add / update a ViewDataDictionary value.
            return true;
        }
    }
}
