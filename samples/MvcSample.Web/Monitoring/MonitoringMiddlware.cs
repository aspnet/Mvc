#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace MvcSample.Web
{
    /// <summary>
    /// Summary description for MonitoringMiddlware
    /// </summary>
    public class MonitoringMiddlware
    {
        private RequestDelegate _next;
        private IServiceProvider _services;

        public MonitoringMiddlware(RequestDelegate next, IServiceProvider services)
        {
            _next = next;
            _services = services;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string url = httpContext.Request.Path.Value;

            if (url.Equals("/Monitoring/Clear", StringComparison.OrdinalIgnoreCase))
            {
                MonitoringModule.Clear();
                httpContext.Response.ContentType = "text/plain";
                var buffer = Encoding.ASCII.GetBytes("Cleared");
                httpContext.Response.Body.Write(buffer, 0, buffer.Length);
            }
            else if (url.Equals("/Monitoring/ActivatedTypes"))
            {
                var data = ActivatedTypes();
                httpContext.Response.ContentType = "text/plain charset=utf-8";
                var buffer = Encoding.UTF8.GetBytes(data);

                httpContext.Response.Body.Write(buffer, 0, buffer.Length);
            }
            else
            {
                await _next(httpContext);
            }
        }

        public string ActivatedTypes()
        {
            var values = MonitoringModule.InstanceCount.ToArray();

            var builder = new StringBuilder();

            Array.Sort(values, new InstancesComparer());

            foreach (var item in values)
            {
                builder.AppendLine(GetTypeName(item.Key.Item1) + " " + item.Value);
            }

            return builder.ToString();
        }

        private string GetTypeName(Type type)
        {
            bool isArray = false;

            if (typeof(Array).IsAssignableFrom(type))
            {
                isArray = true;
            }

            string name = type.Name;

            if (isArray)
            {
                name = ChopLast2(name);
            }

            var genericArgs = type.GetGenericArguments().Select(t => t.Name).ToArray();

            if (genericArgs.Length > 0)
            {
                name = ChopLast2(name) + "<" + string.Join(",", genericArgs) + ">";
            }

            if (isArray)
            {
                name += "[]";
            }

            return name;
        }

        private static string ChopLast2(string name)
        {
            return name.Remove(name.Length - 2);
        }

        public class InstancesComparer : IComparer<KeyValuePair<Tuple<Type, IComponentLifetime>, int>>
        {
            public int Compare(KeyValuePair<Tuple<Type, IComponentLifetime>, int> x, KeyValuePair<Tuple<Type, IComponentLifetime>, int> y)
            {
                if (x.Value < y.Value)
                {
                    return 1;
                }
                else if (x.Value > y.Value)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
#endif