// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="TagHelper"/> base implementation for caching elements.
    /// </summary>
    public class CacheTagKey : IEquatable<CacheTagKey>
    {
        private const string CacheKeyTokenSeparator = "||";
        private static readonly char[] AttributeSeparator = new[] { ',' };

        private string _key;
        private string _prefix;
        private string _varyBy;
        private DateTimeOffset? _expiresOn;
        private TimeSpan? _expiresAfter;
        private TimeSpan? _expiresSliding;
        private IList<KeyValuePair<string, string>> _headers;
        private IList<KeyValuePair<string, string>> _queries;
        private IList<KeyValuePair<string, string>> _routeValues;
        private IList<KeyValuePair<string, string>> _cookies;
        private bool _varyByUser;
        private string _username;

        private string _generatedKey;
        private int? _hashcode;
        
        private CacheTagKey()
        {
        }

        public static CacheTagKey From(CacheTagHelper tagHelper, TagHelperContext context)
        {
            var httpContext = tagHelper.ViewContext.HttpContext;
            var request = httpContext.Request;

            var cacheKey = new CacheTagKey();

            cacheKey._key = context.UniqueId;
            cacheKey._prefix = nameof(CacheTagHelper);

            cacheKey._expiresAfter = tagHelper.ExpiresAfter;
            cacheKey._expiresOn = tagHelper.ExpiresOn;
            cacheKey._expiresSliding = tagHelper.ExpiresSliding;
            cacheKey._varyBy = tagHelper.VaryBy;
            cacheKey._cookies = ExtractCookies(tagHelper.VaryByCookie, request.Cookies);
            cacheKey._headers = ExtractHeaders(tagHelper.VaryByHeader, request.Headers);
            cacheKey._queries = ExtractQueries(tagHelper.VaryByQuery, request.Query);
            cacheKey._routeValues = ExtractRoutes(tagHelper.VaryByRoute, tagHelper.ViewContext.RouteData.Values);
            cacheKey._varyByUser = tagHelper.VaryByUser;

            if (cacheKey._varyByUser)
            {
                cacheKey._username = httpContext.User?.Identity?.Name;
            }

            return cacheKey;
        }

        public static CacheTagKey From(DistributedCacheTagHelper tagHelper, TagHelperContext context)
        {
            var httpContext = tagHelper.ViewContext.HttpContext;
            var request = httpContext.Request;

            var cacheKey = new CacheTagKey();

            cacheKey._key = tagHelper.Name;
            cacheKey._prefix = nameof(DistributedCacheTagHelper);

            cacheKey._expiresAfter = tagHelper.ExpiresAfter;
            cacheKey._expiresOn = tagHelper.ExpiresOn;
            cacheKey._expiresSliding = tagHelper.ExpiresSliding;
            cacheKey._varyBy = tagHelper.VaryBy;
            cacheKey._cookies = ExtractCookies(tagHelper.VaryByCookie, request.Cookies);
            cacheKey._headers = ExtractHeaders(tagHelper.VaryByHeader, request.Headers);
            cacheKey._queries = ExtractQueries(tagHelper.VaryByQuery, request.Query);
            cacheKey._routeValues = ExtractRoutes(tagHelper.VaryByRoute, tagHelper.ViewContext.RouteData.Values);
            cacheKey._varyByUser = tagHelper.VaryByUser;

            if (cacheKey._varyByUser)
            {
                cacheKey._username = httpContext.User?.Identity?.Name;
            }

            return cacheKey;
        }

        private static IList<KeyValuePair<string, string>> ExtractCookies(string keys, IRequestCookieCollection cookies)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return null;
            }

            var values = Tokenize(keys);

            if (values.Count == 0)
            {
                return null;
            }

            var result = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                var cookie = cookies[item];
                if (!string.IsNullOrEmpty(cookie))
                {
                    result.Add(new KeyValuePair<string, string>(item, cookie));
                }
            }

            return result;
        }

        private static IList<KeyValuePair<string, string>> ExtractHeaders(string keys, IHeaderDictionary headers)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return null;
            }

            var values = Tokenize(keys);

            if (values.Count == 0)
            {
                return null;
            }

            var result = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                var header = headers[item];
                if (!string.IsNullOrEmpty(header))
                {
                    result.Add(new KeyValuePair<string, string>(item, header));
                }
            }

            return result;
        }

        private static IList<KeyValuePair<string, string>> ExtractQueries(string keys, IQueryCollection queries)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return null;
            }

            var values = Tokenize(keys);

            if (values.Count == 0)
            {
                return null;
            }

            var result = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                var query = queries[item];
                if (!string.IsNullOrEmpty(query))
                {
                    result.Add(new KeyValuePair<string, string>(item, query));
                }
            }

            return result;
        }

        private static IList<KeyValuePair<string, string>> ExtractRoutes(string keys, RouteValueDictionary routeValues)
        {
            if (string.IsNullOrEmpty(keys))
            {
                return null;
            }

            var values = Tokenize(keys);

            if (values.Count == 0)
            {
                return null;
            }

            var result = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                var routeValue = routeValues[item];
                if (routeValue != null)
                {
                    result.Add(new KeyValuePair<string, string>(item, routeValue.ToString()));
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of the key.
        /// </summary>
        /// <returns>A <see cref="string"/> uniquely representing the key.</returns>
        public string GenerateKey()
        {
            if (_generatedKey != null)
            {
                return _generatedKey;
            }

            var builder = new StringBuilder(_prefix);
            builder
                .Append(CacheKeyTokenSeparator)
                .Append(_key);

            if (!string.IsNullOrEmpty(_varyBy))
            {
                builder
                    .Append(CacheKeyTokenSeparator)
                    .Append(nameof(_varyBy))
                    .Append(CacheKeyTokenSeparator)
                    .Append(_varyBy);
            }

            AddStringCollection(builder, nameof(_cookies), _cookies);
            AddStringCollection(builder, nameof(_headers), _headers);
            AddStringCollection(builder, nameof(_queries), _queries);
            AddStringCollection(builder, nameof(_routeValues), _routeValues);

            if (_varyByUser)
            {
                builder
                    .Append(CacheKeyTokenSeparator)
                    .Append(nameof(_varyByUser))
                    .Append(CacheKeyTokenSeparator)
                    .Append(_username);
            }

            return _generatedKey = builder.ToString();
        }

        /// <summary>
        /// Creates a hashed value of the key.
        /// </summary>
        /// <returns>A cryptographic hash of the key.</returns>
        public string GenerateHashedKey()
        {
            var key = GenerateKey();

            // The key is typically too long to be useful, so we use a cryptographic hash
            // as the actual key (better randomization and key distribution, so small vary
            // values will generate dramatically different keys).
            using (var sha = SHA256.Create())
            {
                var contentBytes = Encoding.UTF8.GetBytes(key);
                var hashedBytes = sha.ComputeHash(contentBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static void AddStringCollection(
            StringBuilder builder,
            string collectionName,
            IList<KeyValuePair<string, string>> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            // keyName(param1=value1|param2=value2)
            builder
                .Append(CacheKeyTokenSeparator)
                .Append(collectionName)
                .Append("(");

            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];

                builder
                    .Append(item.Key)
                    .Append(CacheKeyTokenSeparator)
                    .Append(item.Value)
                    .Append(CacheKeyTokenSeparator);
            }
            
            builder.Append(")");
        }

        protected static IList<string> Tokenize(string value)
        {
            var values = value.Split(AttributeSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
            {
                return values;
            }

            var trimmedValues = new List<string>();

            for (var i = 0; i < values.Length; i++)
            {
                var trimmedValue = values[i].Trim();

                if (trimmedValue.Length > 0)
                {
                    trimmedValues.Add(trimmedValue);
                }
            }

            return trimmedValues;
        }

        private static int ComputeValuesHashCode(
            int hash,
            string collectionName,
            IList<KeyValuePair<string, string>> values)
        {
            hash = hash * 23 + collectionName.GetHashCode();

            if (values != null)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    var item = values[i];
                    hash = hash * 23 + item.Key.GetHashCode();
                    hash = hash * 23 + item.Value.GetHashCode();
                }
            }

            return hash;
        }

        public override int GetHashCode()
        {
            if (_hashcode.HasValue)
            {
                return _hashcode.Value;
            }

            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + _key.GetHashCode();
                hash = hash * 23 + _expiresAfter.GetHashCode();
                hash = hash * 23 + _expiresOn.GetHashCode();
                hash = hash * 23 + _expiresSliding.GetHashCode();
                hash = hash * 23 + _varyBy.GetHashCode();
                hash = hash * 23 + _username.GetHashCode();

                hash = ComputeValuesHashCode(hash, nameof(_cookies), _cookies);
                hash = ComputeValuesHashCode(hash, nameof(_headers), _headers);
                hash = ComputeValuesHashCode(hash, nameof(_queries), _queries);
                hash = ComputeValuesHashCode(hash, nameof(_routeValues), _routeValues);

                _hashcode = hash;
                
                return hash;
            }
        }

        public bool Equals(CacheTagKey other)
        {
            if (other._key != _key ||
                other._expiresAfter != _expiresAfter ||
                other._expiresOn != _expiresOn ||
                other._expiresSliding != _expiresSliding ||
                other._varyBy != _varyBy ||
                !AreSame(_cookies, other._cookies) ||
                !AreSame(_headers, other._headers) ||
                !AreSame(_queries, other._queries) ||
                !AreSame(_routeValues, other._routeValues) ||
                _varyByUser != other._varyByUser ||
                _varyByUser && _username != other._username
                )
            {
                return false;
            }

            return false;
        }

        private static bool AreSame(IList<KeyValuePair<string, string>> values1, IList<KeyValuePair<string, string>> values2)
        {
            if (values1 == values2)
            {
                return true;
            }

            if (values1 == null || values2 == null)
            {
                return false;
            }

            if (values1.Count != values2.Count)
            {
                return false;
            }

            for (var i = 0; i < values1.Count; i++)
            {
                if (values1[i].Key != values2[i].Key ||
                    values1[i].Value != values2[i].Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}