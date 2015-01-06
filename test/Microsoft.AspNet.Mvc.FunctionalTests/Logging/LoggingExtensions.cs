﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public static ScopeNodeDto FindScope(this IEnumerable<ActivityContextDto> activities,
                                                                    string scopeName)
        {
            ScopeNodeDto node = null;

            foreach (var activity in activities)
            {
                if (activity.RepresentsScope)
                {
                    node = GetScope(activity.Root, scopeName);

                    // Ideally we do not expect multiple scopes with the same name
                    // to exist in the logs, so we break on the first found scope node.
                    // Note: The logs can contain multiple scopes with the same name across
                    // different requests, but the tests are expected to filter the logs by request
                    // (ex: using request trace id) and then find the scope by name.
                    if (node != null)
                    {
                        return node;
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Gets all the logs messages matching the given structure type
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="structureType"></param>
        /// <returns></returns>
        public static IEnumerable<LogInfoDto> GetLogsByStructureType(this IEnumerable<ActivityContextDto> activities,
                                                                        Type structureType)
        {
            var logInfos = new List<LogInfoDto>();
            foreach (var activity in activities)
            {
                if (!activity.RepresentsScope)
                {
                    var logInfo = activity.Root.Messages.OfStructureType(structureType)
                                                        .FirstOrDefault();

                    if (logInfo != null)
                    {
                        logInfos.Add(logInfo);
                    }
                }
                else
                {
                    GetLogsByStructureType(activity.Root, logInfos, structureType);
                }
            }

            return logInfos;
        }

        /// <summary>
        /// Filters for logs activties created during application startup
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        public static IEnumerable<ActivityContextDto> FilterByStartup(this IEnumerable<ActivityContextDto> activities)
        {
            return activities.Where(activity => activity.RequestInfo == null);
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
        /// Filters the log messages based on the given structure type
        /// </summary>
        /// <param name="logInfos"></param>
        /// <param name="structureType"></param>
        /// <returns></returns>
        public static IEnumerable<LogInfoDto> OfStructureType(this IEnumerable<LogInfoDto> logInfos,
                                                            Type structureType)
        {
            return logInfos.Where(logInfo => logInfo.StateType != null
                                            && logInfo.StateType.Equals(structureType));
        }

        /// <summary>
        /// Traverses through the log node tree and gets the log messages whose StateType
        /// matches the supplied structure type.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="logInfoDtos"></param>
        /// <param name="structureType"></param>
        private static void GetLogsByStructureType(ScopeNodeDto node, IList<LogInfoDto> logInfoDtos, Type structureType)
        {
            foreach (var logInfo in node.Messages.OfStructureType(structureType))
            {
                logInfoDtos.Add(logInfo);
            }

            foreach (var scopeNode in node.Children)
            {
                GetLogsByStructureType(scopeNode, logInfoDtos, structureType);
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