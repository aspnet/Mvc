// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Net.Http.Headers;
using Resources = Microsoft.AspNetCore.Mvc.Core.Resources;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that specifies the supported request content types. <see cref="ContentTypes"/> is used to select an
    /// action when there would otherwise be multiple matches.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ConsumesAttribute :
        Attribute,
        IResourceFilter,
        IConsumesActionConstraint,
        IApiRequestMetadataProvider
    {
        public static readonly int ConsumesActionConstraintOrder = 200;

        /// <summary>
        /// Creates a new instance of <see cref="ConsumesAttribute"/>.
        /// </summary>
        public ConsumesAttribute(string contentType, params string[] otherContentTypes)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            ContentTypes = new MediaTypeCollection
            {
                ParseMediaTypeHeaderValue(contentType),
            };

            for (var i = 0; i < otherContentTypes.Length; i++)
            {
                ContentTypes.Add(ParseMediaTypeHeaderValue(otherContentTypes[i]));
            }

            MediaTypeHeaderValue ParseMediaTypeHeaderValue(string value)
            {
                // We want to ensure that the given provided content types are valid values, so
                // we validate them using the semantics of MediaTypeHeaderValue.
                var mediaType = MediaTypeHeaderValue.Parse(value);
                if (mediaType.MatchesAllSubTypes || mediaType.MatchesAllTypes)
                {
                    throw new InvalidOperationException(Resources.FormatMatchAllContentTypeIsNotAllowed(mediaType));
                }

                return mediaType;
            }
        }

        // The value used is a non default value so that it avoids getting mixed with other action constraints
        // with default order.
        /// <inheritdoc />
        int IActionConstraint.Order => ConsumesActionConstraintOrder;

        /// <summary>
        /// Gets or sets the supported request content types. Used to select an action when there would otherwise be
        /// multiple matches.
        /// </summary>
        public MediaTypeCollection ContentTypes { get; set; }

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Only execute if the current filter is the one which is closest to the action.
            // Ignore all other filters. This is to ensure we have a overriding behavior.
            if (!IsEffectivePolicy(context.ActionDescriptor))
            {
                return;
            }

            var requestContentType = context.HttpContext.Request.ContentType;

            // Confirm the request's content type is more specific than a media type this action supports e.g. OK
            // if client sent "text/plain" data and this action supports "text/*".
            if (requestContentType != null && !IsSubsetOfAnyContentType(requestContentType))
            {
                context.Result = new UnsupportedMediaTypeResult();
            }
        }

        private bool IsSubsetOfAnyContentType(string requestMediaType)
        {
            var parsedRequestMediaType = new MediaType(requestMediaType);
            for (var i = 0; i < ContentTypes.Count; i++)
            {
                var contentTypeMediaType = new MediaType(ContentTypes[i]);
                if (parsedRequestMediaType.IsSubsetOf(contentTypeMediaType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        /// <inheritdoc />
        public bool Accept(ActionConstraintContext context)
        {
            if (!IsEffectivePolicy(context.CurrentCandidate.Action))
            {
                // Since the constraint is to be skipped, returning true here
                // will let the current candidate ignore this constraint and will
                // be selected based on other constraints for this action.
                return true;
            }

            var requestContentType = context.RouteContext.HttpContext.Request.ContentType;
            if (requestContentType == null)
            {
                var isActionWithoutConsumeConstraintPresent = context.Candidates.Any(
                    candidate => candidate.Constraints == null ||
                    !candidate.Constraints.Any(constraint => constraint is IConsumesActionConstraint));

                return !isActionWithoutConsumeConstraintPresent;
            }

            if (IsSubsetOfAnyContentType(requestContentType))
            {
                return true;
            }

            // The current action isn't a candidate. The next course of action is to determine if there is another
            // candidate with an IConsumesActionConstraint filter that accept the current request. There are two possibilities
            // a) None of the candidates match. We'll return a 415 from the first candidate.
            // b) Another candidate will eventually match.
            // Only the first candidate needs to probe the other candidates to determine this.

            var firstCandidate = context.Candidates[0];
            if (firstCandidate.Action != context.CurrentCandidate.Action)
            {
                return false;
            }

            for (var i = 1; i < context.Candidates.Count; i++)
            {
                var candidate = context.Candidates[i];

                var tempContext = new ActionConstraintContext()
                {
                    Candidates = context.Candidates,
                    RouteContext = context.RouteContext,
                    CurrentCandidate = candidate
                };

                if (candidate.Constraints == null ||
                    candidate.Constraints.Count == 0 ||
                    candidate.Constraints.OfType<IConsumesActionConstraint>().Any(constraint => constraint.Accept(tempContext)))
                {
                    // There is another candidate in the chain that can handle the request.
                    return false;
                }
            }

            return true;
        }

        private bool IsEffectivePolicy(ActionDescriptor actionDescriptor)
        {
            // The most specific policy is the one closest to the action (nearest the end of the list).
            var filterDescriptors = actionDescriptor.FilterDescriptors;
            for (var i = filterDescriptors.Count - 1; i >= 0; i--)
            {
                var filter = filterDescriptors[i].Filter;
                if (filter is IConsumesActionConstraint)
                {
                    return object.ReferenceEquals(filter, this);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void SetContentTypes(MediaTypeCollection contentTypes)
        {
            contentTypes.Clear();
            foreach (var contentType in ContentTypes)
            {
                contentTypes.Add(contentType);
            }
        }
    }
}