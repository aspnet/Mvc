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

        /// <summary>
        /// Gets a scope node with the given name
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="scopeName"></param>
        /// <returns>A scope node if found, else null</returns>
        public static ScopeNodeDto FindScopeByName(this IEnumerable<ActivityContextDto> activities,
                                                                    string scopeName)
        {
            ScopeNodeDto node = null;

            foreach(var activity in activities)
            {
                if(activity.RepresentsScope)
                {
                    node = GetScope(activity.Root, scopeName);

                    // Ideally we do not expect multiple scopes with the same name
                    // to exist in the logs, so we break on the first found scope node.
                    // Note: The logs can contain multiple scopes with the same name across
                    // different requests, but the tests are expected to filter the logs by request
                    // (ex: using request trace id) and then find the scope by name.
                    if (node != null)
                    {
                        break;
                    }                    
                }
            }

            return node;                        
        }
        
        /// <summary>
        /// Gets a flattened list of logs that are logged at application's startup. 
        /// </summary>
        /// <param name="activities"></param>
        /// <returns>List of log messages</returns>
        public static IEnumerable<LogInfoDto> GetStartupLogs(this IEnumerable<ActivityContextDto> activities)
        {
            return activities.Where(activity => activity.RequestInfo == null)
                             .GetLogs();
        }

        /// <summary>
        /// Get a flattened list of logs present within a given scope and that are logged 
        /// at application's startup.
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="scopeName"></param>
        /// <returns>List of log messages</returns>
        public static IEnumerable<LogInfoDto> GetStartupLogs(this IEnumerable<ActivityContextDto> activities,
                                                            string scopeName)
        {
            return activities.Where(activity => activity.RequestInfo == null)
                             .GetLogsUnderScope(scopeName);
        }

        /// <summary>
        /// Gets a flattened list of logs that are logged for a given request 
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="requestTraceId">The "RequestTraceId" query parameter value</param>
        /// <returns>List of log messages</returns>
        public static IEnumerable<LogInfoDto> GetLogs(this IEnumerable<ActivityContextDto> activities,
                                                     string requestTraceId)
        {
            return activities.FilterByRequestTraceId(requestTraceId)
                             .GetLogs();
        }

        /// <summary>
        /// Get a flattened list of logs present within a given scope and that are logged 
        /// for a given request
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="requestTraceId">The "RequestTraceId" query parameter value</param>
        /// <param name="scopeName"></param>
        /// <returns></returns>
        public static IEnumerable<LogInfoDto> GetLogs(this IEnumerable<ActivityContextDto> activities,
                                                     string requestTraceId,
                                                     string scopeName)
        {
            return activities.FilterByRequestTraceId(requestTraceId)
                             .GetLogsUnderScope(scopeName);
        }

        /// <summary>
        /// Filters log activities based on the given request.
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="requestTraceId">The "RequestTraceId" query parameter value</param>
        /// <returns></returns>
        public static IEnumerable<ActivityContextDto> FilterByRequestTraceId(this IEnumerable<ActivityContextDto> activities,
                                                                                string requestTraceId)
        {
            return activities.Where(activity => activity.RequestInfo != null
                        && string.Equals(GetQueryValue(activity.RequestInfo.Query, RequestTraceIdQueryKey),
                                        requestTraceId,
                                        StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Compares two trees and verifies if the scope nodes are equal
        /// </summary>
        /// <param name="root1"></param>
        /// <param name="root2"></param>
        /// <returns></returns>
        public static bool AreScopesEqual(ScopeNodeDto root1, ScopeNodeDto root2)
        {
            if (root1 == null && root2 == null) return true;

            if (root1 == null || root2 == null) return false;

            if(!string.Equals(root1.State?.ToString(), root2.State?.ToString())
                || root1.Children.Count != root2.Children.Count)
            {
                return false;
            }

            bool isChildScopeEqual = true;
            for(int i = 0; i < root1.Children.Count; i++)
            {
                isChildScopeEqual = AreScopesEqual(root1.Children[i], root2.Children[i]);

                if (!isChildScopeEqual) break;
            }

            return isChildScopeEqual;
        }

        private static IEnumerable<LogInfoDto> GetLogsUnderScope(this IEnumerable<ActivityContextDto> activities,
                                                        string scopeName)
        {
            var logInfos = new List<LogInfoDto>();

            foreach (var activity in activities)
            {
                if (activity.RepresentsScope)
                {
                    GetLogsUnderScope(activity.Root, logInfos, scopeName, foundScope: false);
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

        private static void GetLogsUnderScope(ScopeNodeDto node,
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
                GetLogsUnderScope(scopeNode, logInfoDtos, scopeName, foundScope);
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