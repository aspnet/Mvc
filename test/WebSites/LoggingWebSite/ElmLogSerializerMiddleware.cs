using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggingWebSite.Models;
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

            // Filter the logs.
            // For logs which are generated during the application's startup,
            // they would not be associated with a RequestId that the ElmCapture middleware
            // creates.
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

        private void GetLogDetails(ElmStore elmStore)
        {
            var activities = new List<ActivityContextDto>();
            foreach (var activity in elmStore.GetActivities().Reverse())
            {
                activities.Add(new ActivityContextDto()
                {
                    HttpInfo = GetRequestInfoDto(activity.HttpInfo),
                    Id = activity.Id,
                    RepresentsScope = activity.RepresentsScope,

                });
            }
        }

        private RequestInfoDto GetRequestInfoDto(HttpInfo httpInfo)
        {
            return new RequestInfoDto()
            {
                ContentType = httpInfo.ContentType,
                Cookies = httpInfo.Cookies,
                Headers = httpInfo.Headers,
                Host = httpInfo.Host.Value,
                Method = httpInfo.Method,
                Path = httpInfo.Path.Value,
                Protocol = httpInfo.Protocol,
                Query = httpInfo.Query.Value,
                RequestID = httpInfo.RequestID,
                Scheme = httpInfo.Scheme,
                StatusCode = httpInfo.StatusCode
            };
        }

        private LogInfoDto GetLogInfoDto(LogInfo logInfo)
        {
            return new LogInfoDto()
            {
                EventID = logInfo.EventID,
                Exception = logInfo.Exception,
                LoggerName = logInfo.Name,
                LogLevel = logInfo.Severity,
                State = logInfo.State,
                StateType = logInfo.State?.GetType()
            };
        }

        private ScopeNodeDto CopyScopeNodeTree(ScopeNode root, ScopeNodeDto rootDto)
        {
            rootDto.LoggerName = root.Name;

            foreach (var logInfo in root.Messages)
            {
                rootDto.Messages.Add(GetLogInfoDto(logInfo));
            }

            foreach (var scopeNode in root.Children)
            {
                ScopeNodeDto childDto = new ScopeNodeDto();
                childDto.Parent = rootDto;

                rootDto.Children.Add(CopyScopeNodeTree(scopeNode, childDto));
            }

            return rootDto;
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


        //private class SDTO
        //{
        //    public List<MDTO> Messages { get; } = new List<MDTO>();

        //    public List<SDTO> Children { get; } = new List<SDTO>();
        //}

        //private class MDTO
        //{
        //}


        //private SDTO T(ScopeNode node)
        //{
        //    var result = new SDTO();
        //    // copy properties

        //    foreach (var message in node.Messages)
        //    {
        //        var m = new MDTO();
        //        // copy properties

        //        result.Messages.Add(m);
        //    }

        //    foreach (var child in node.Children)
        //    {
        //        var c = T(child);
        //        result.Children.Add(T(child));
        //    }

        //    return result;
        //}
    }
}