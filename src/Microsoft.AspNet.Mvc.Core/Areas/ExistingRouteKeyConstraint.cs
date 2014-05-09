// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc
{
    public class ExistingRouteKeyConstraint : IRouteConstraint
    {
        private readonly string _routeKey;

        public ExistingRouteKeyConstraint([NotNull]string routeKey)
        {
            _routeKey = routeKey;
        }

        public bool Match([NotNull]HttpContext httpContext,
                          [NotNull]IRouter route,
                          [NotNull]string routeKey,
                          [NotNull]IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            object value = null;
            if (values.TryGetValue(_routeKey, out value))
            {
                string valueAsString = value as string;

                if (valueAsString != null)
                {
                    var allValues = GetAllValues(httpContext);
                    var match = allValues.Any(existingRouteValue => existingRouteValue.Equals(valueAsString, StringComparison.OrdinalIgnoreCase));

                    return match;
                }
            }

            return false;
        }

        private string[] GetAllValues(HttpContext httpContext)
        {
            var provider = httpContext.ApplicationServices.GetService<INestedProviderManager<ActionDescriptorProviderContext>>();
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var allAreas = context
                            .Results
                            .Select(ad => ad.RouteConstraints
                                            .FirstOrDefault(c => c.RouteKey == _routeKey && 
                                                            c.KeyHandling == RouteKeyHandling.RequireKey))
                            .Where(rc => rc != null)
                            .Select(rc => rc.RouteValue)
                            .Distinct()
                            .ToArray();

            return allAreas;
        }
    }
}