using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Elm;
using Microsoft.AspNet.Http;
using Newtonsoft.Json;

namespace LoggingWebSite
{
    public class ElmLogSerializerMiddleware
    {
        public const string StartupHeaderKey = "Startup";
        public const string RequestTraceIdHeaderKey = "RequestTraceID";

        private readonly RequestDelegate _next;

        public ElmLogSerializerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, ElmStore elmStore)
        {
            var currentRequest = context.Request;

            var logInfos = GetAllLogInfos(elmStore);

            // filter the logs
            // For logs which are generated during the application's startup,
            // they would not be associated with a RequestID
            if (currentRequest.Headers.ContainsKey(StartupHeaderKey))
            {
                logInfos = logInfos.Where(info =>
                                    {
                                        return (info.ActivityContext != null
                                                && (info.ActivityContext.HttpInfo == null
                                                || info.ActivityContext.HttpInfo.RequestID == Guid.Empty));
                                    });
            }
            // Filter by client's request trace id
            else if (currentRequest.Headers.ContainsKey(RequestTraceIdHeaderKey))
            {
                logInfos = logInfos.Where(info =>
                                        {
                                            return (info.ActivityContext != null
                                                && info.ActivityContext.HttpInfo != null
                                                && string.Equals(
                                                        info.ActivityContext.HttpInfo.Headers[RequestTraceIdHeaderKey],
                                                        currentRequest.Headers[RequestTraceIdHeaderKey],
                                                        StringComparison.OrdinalIgnoreCase));
                                        });
            }

            // convert the log infos to DTOs to be able to be transferred over the wire to tests
            var logInfoDtos = new List<LogInfoDto>();
            foreach (var logInfo in logInfos)
            {
                logInfoDtos.Add(new LogInfoDto()
                {
                    EventID = logInfo.EventID,
                    Exception = logInfo.Exception,
                    LoggerName = logInfo.Name,
                    LogLevel = logInfo.Severity,
                    State = logInfo.State,
                    StateType = logInfo.State?.GetType()
                });
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var serializer = JsonSerializer.Create();
            using (var writer = new JsonTextWriter(new StreamWriter(stream: context.Response.Body,
                                                                    encoding: Encoding.UTF8,
                                                                    bufferSize: 1024,
                                                                    leaveOpen: true)))
            {
                serializer.Serialize(writer, logInfoDtos);
            }

            return Task.FromResult(true);
        }


        // Elm logs are arranged in the form of activities. Each activity could
        // represent a tree of nodes. So here we traverse through the tree to get a flat list of
        // log messages for us to enable verifying in the test.
        private IEnumerable<LogInfo> GetAllLogInfos(ElmStore elmStore)
        {
            // Build a flat list of log messages from the log node tree 
            var logInfos = new List<LogInfo>();
            foreach (var activity in elmStore.GetActivities().Reverse())
            {
                if (!activity.RepresentsScope)
                {
                    // message not within a scope
                    var logInfo = activity.Root.Messages.FirstOrDefault();
                    logInfos.Add(logInfo);
                }
                else
                {
                    Traverse(activity.Root, logInfos);
                }
            }

            return logInfos;
        }

        private void Traverse(ScopeNode node, IList<LogInfo> logInfos)
        {
            foreach (var logInfo in node.Messages)
            {
                logInfos.Add(logInfo);
            }

            foreach (var scopeNode in node.Children)
            {
                Traverse(scopeNode, logInfos);
            }
        }
    }
}