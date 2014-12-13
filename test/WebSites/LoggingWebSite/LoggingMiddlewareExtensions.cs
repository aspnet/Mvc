using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace LoggingWebSite
{
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder, TestSink sink)
        {
            // add the test logger provider to the factory here so the logger can start capturing logs
            var factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            factory.AddProvider(new TestLoggerProvider(sink));

            return builder.UseMiddleware<LoggingMiddleware>(sink);
        }
    }
}