// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Enumerates the HTML verbs.
    /// </summary>
    public enum HttpVerbs
    {
        /// <summary>
        /// Retrieves the information or entity that is identified by the URI of the request.
        /// </summary>
        Get,
        /// <summary>
        /// Posts a new entity as an addition to a URI.
        /// </summary>
        Post,
        /// <summary>
        /// Replaces an entity that is identified by a URI.
        /// </summary>
        Put,
        /// <summary>
        /// Requests that a specified URI be deleted.
        /// </summary>
        Delete,
        /// <summary>
        /// Retrieves the message headers for the information or entity that is identified by the URI of the request.
        /// </summary>
        Head,
        /// <summary>
        /// Requests that a set of changes described in the request entity be applied to the resource identified by the
        /// Request- URI.
        /// </summary>
        Patch,
        /// <summary>
        /// Represents a request for information about the communication options available on the request/response
        /// chain identified by the Request-URI.
        /// </summary>
        Options,
    }
}
