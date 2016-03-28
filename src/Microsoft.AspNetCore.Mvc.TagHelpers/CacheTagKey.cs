// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Internal;

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
            var cacheKey = FromCore(tagHelper, context);

            cacheKey._key = context.UniqueId;
            cacheKey._prefix = nameof(CacheTagHelper);

            return cacheKey;
        }

        public static CacheTagKey From(DistributedCacheTagHelper tagHelper, TagHelperContext context)
        {
            var cacheKey = FromCore(tagHelper, context);

            cacheKey._key = tagHelper.Name;
            cacheKey._prefix = nameof(DistributedCacheTagHelper);

            return cacheKey;
        }

        private static CacheTagKey FromCore(CacheTagHelperBase tagHelper, TagHelperContext context)
        {
            var cacheKey = new CacheTagKey();

            var httpContext = tagHelper.ViewContext.HttpContext;
            var request = httpContext.Request;

            cacheKey._expiresAfter = tagHelper.ExpiresAfter;
            cacheKey._expiresOn = tagHelper.ExpiresOn;
            cacheKey._expiresSliding = tagHelper.ExpiresSliding;
            cacheKey._varyBy = tagHelper.VaryBy;
            cacheKey._cookies = ExtractCollection(tagHelper.VaryByCookie, request.Cookies, (c, key) => c[key]);
            cacheKey._headers = ExtractCollection(tagHelper.VaryByHeader, request.Headers, (c, key) => c[key]);
            cacheKey._queries = ExtractCollection(tagHelper.VaryByQuery, request.Query, (c, key) => c[key]);
            cacheKey._routeValues = ExtractCollection(tagHelper.VaryByRoute, tagHelper.ViewContext.RouteData.Values, (c, key) => c[key].ToString());
            cacheKey._varyByUser = tagHelper.VaryByUser;

            if (cacheKey._varyByUser)
            {
                cacheKey._username = httpContext.User?.Identity?.Name;
            }

            return cacheKey;
        }

        private static IList<KeyValuePair<string, string>> ExtractCollection<TSourceCollection>(string keys, TSourceCollection collection, Func<TSourceCollection, string, string> accessor)
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
                var value = accessor(collection, item);
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(new KeyValuePair<string, string>(item, value));
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

        private static void CombineCollectionHashCode(
            HashCodeCombiner hashCodeCombiner,
            string collectionName,
            IList<KeyValuePair<string, string>> values)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    var item = values[i];
                    hashCodeCombiner.Add(item.Key);
                    hashCodeCombiner.Add(item.Value);
                }
            }
        }

        public override int GetHashCode()
        {
            if (_hashcode.HasValue)
            {
                return _hashcode.Value;
            }

            var hashCodeCombiner = new HashCodeCombiner();

            hashCodeCombiner.Add(_key);
            hashCodeCombiner.Add(_expiresAfter);
            hashCodeCombiner.Add(_expiresOn);
            hashCodeCombiner.Add(_expiresSliding);
            hashCodeCombiner.Add(_varyBy);
            hashCodeCombiner.Add(_username);
                
            CombineCollectionHashCode(hashCodeCombiner, nameof(_cookies), _cookies);
            CombineCollectionHashCode(hashCodeCombiner, nameof(_headers), _headers);
            CombineCollectionHashCode(hashCodeCombiner, nameof(_queries), _queries);
            CombineCollectionHashCode(hashCodeCombiner, nameof(_routeValues), _routeValues);

            return hashCodeCombiner;
        }

        public bool Equals(CacheTagKey other)
        {
            if (!string.Equals(other._key, _key) ||
                other._expiresAfter != _expiresAfter ||
                other._expiresOn != _expiresOn ||
                other._expiresSliding != _expiresSliding ||
                !string.Equals(other._varyBy, _varyBy) ||
                !AreSame(_cookies, other._cookies) ||
                !AreSame(_headers, other._headers) ||
                !AreSame(_queries, other._queries) ||
                !AreSame(_routeValues, other._routeValues) ||
                _varyByUser != other._varyByUser ||
                (_varyByUser && !string.Equals(other._username, _username, StringComparison.Ordinal))
                )
            {
                return false;
            }

            return true;
        }

        private static bool AreSame(IList<KeyValuePair<string, string>> values1, IList<KeyValuePair<string, string>> values2)
        {
            if (values1 == values2)
            {
                return true;
            }

            if (values1 == null || values2 == null || values1.Count != values2.Count)
            {
                return false;
            }

            for (var i = 0; i < values1.Count; i++)
            {
                if (!string.Equals(values1[i].Key, values2[i].Key, StringComparison.Ordinal) ||
                    !string.Equals(values1[i].Value, values2[i].Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}