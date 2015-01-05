using System;
using System.Linq;
using System.Collections.Generic;
using LoggingWebSite;
using Microsoft.AspNet.WebUtilities;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public static class LoggingExtensions
    {
        public const string RequestTraceIdQueryKey = "RequestTraceId";

        public static ScopeNodeDto FindScopeWithName(this IEnumerable<ActivityContextDto> activities,
                                                                    string scopeName)
        {
            ScopeNodeDto node = null;

            foreach(var activity in activities)
            {
                if(activity.RepresentsScope)
                {
                    node = GetScope(activity.Root, scopeName);
                    
                    if(node != null)
                    {
                        break;
                    }                    
                }
            }

            return node;                        
        }
        
        public static IEnumerable<LogInfoDto> GetStartupLogs(this IEnumerable<ActivityContextDto> activities)
        {
            return activities.Where(activity => activity.RequestInfo == null)
                             .GetLogs();
        }

        public static IEnumerable<LogInfoDto> GetStartupLogs(this IEnumerable<ActivityContextDto> activities,
                                                            string scopeName)
        {
            return activities.Where(activity => activity.RequestInfo == null)
                             .GetLogsUnderScope(scopeName);
        }

        public static IEnumerable<LogInfoDto> GetLogs(this IEnumerable<ActivityContextDto> activities,
                                                     string requestTraceId)
        {
            return activities.FilterByRequestTraceId(requestTraceId)
                             .GetLogs();
        }

        public static IEnumerable<LogInfoDto> GetLogs(this IEnumerable<ActivityContextDto> activities,
                                                     string requestTraceId,
                                                     string scopeName)
        {
            return activities.FilterByRequestTraceId(requestTraceId)
                             .GetLogsUnderScope(scopeName);
        }

        private static IEnumerable<LogInfoDto> GetLogsUnderScope(this IEnumerable<ActivityContextDto> activities,
                                                        string scopeName)
        {
            var logInfos = new List<LogInfoDto>();

            foreach (var activity in activities)
            {
                if (activity.RepresentsScope)
                {
                    GetLogsUnderScopeHelper(activity.Root, logInfos, scopeName, foundScope: false);
                }
            }

            return logInfos;
        }

        private static IEnumerable<LogInfoDto> GetLogs(this IEnumerable<ActivityContextDto> activities)
        {
            // Build a flat list of log messages from the log node tree 
            var logInfos = new List<LogInfoDto>();
            foreach (var activity in activities)
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

        private static void GetLogsUnderScopeHelper(ScopeNodeDto node,
                                    IList<LogInfoDto> logInfoDtos,
                                    string scopeName,
                                    bool foundScope)
        {
            if (!foundScope
                && string.Equals(node.State?.ToString(), scopeName, StringComparison.OrdinalIgnoreCase))
            {
                foundScope = true;
            }

            if (foundScope)
            {
                foreach (var logInfo in node.Messages)
                {
                    logInfoDtos.Add(logInfo);
                }
            }

            foreach (var scopeNode in node.Children)
            {
                GetLogsUnderScopeHelper(scopeNode, logInfoDtos, scopeName, foundScope);
            }
        }
        
        private static void Traverse(ScopeNodeDto node, IList<LogInfoDto> logInfoDtos)
        {
            foreach (var logInfo in node.Messages)
            {
                logInfoDtos.Add(logInfo);
            }

            foreach (var scopeNode in node.Children)
            {
                Traverse(scopeNode, logInfoDtos);
            }
        }

        private static IEnumerable<ActivityContextDto> FilterByRequestTraceId(this IEnumerable<ActivityContextDto> activities,
                                                                                string requestTraceId)
        {
            return activities.Where(activity => activity.RequestInfo != null
                        && string.Equals(GetQueryValue(activity.RequestInfo.Query, RequestTraceIdQueryKey),
                                        requestTraceId,
                                        StringComparison.OrdinalIgnoreCase));
        }

        private static ScopeNodeDto GetScope(ScopeNodeDto root, string scopeName)
        {
            if (string.Equals(root.State?.ToString(),
                            scopeName,
                            StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            foreach (var childNode in root.Children)
            {
                var foundNode = GetScope(childNode, scopeName);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        private static string GetQueryValue(string query, string key)
        {
            var queryString = QueryHelpers.ParseQuery(query);

            return queryString[key];
        }
    }
}