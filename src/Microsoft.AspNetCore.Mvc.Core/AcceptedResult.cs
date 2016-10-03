// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns an Accepted (202) response with a Location header.
    /// </summary>
    public class AcceptedResult : ObjectResult
    {
        private string _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptedResult"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="location">The location at which the status of requested content can be monitored.</param>
        /// <param name="value">The value to format in the entity body.</param>
        public AcceptedResult(string location, object value)
            : base(value)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            Location = location;
            StatusCode = StatusCodes.Status202Accepted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptedResult"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="location">The location at which the status of requested content can be monitored.</param>
        /// <param name="value">The value to format in the entity body.</param>
        public AcceptedResult(Uri location, object value)
            : base(value)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.IsAbsoluteUri)
            {
                Location = location.AbsoluteUri;
            }
            else
            {
                Location = location.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
            }

            StatusCode = StatusCodes.Status202Accepted;
        }

        /// <summary>
        /// Gets or sets the location at which the status of the requested content can be monitored.
        /// </summary>
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _location = value;
            }
        }

        /// <inheritdoc />
        public override void OnFormatting(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            base.OnFormatting(context);

            context.HttpContext.Response.Headers[HeaderNames.Location] = Location;
        }
    }
}