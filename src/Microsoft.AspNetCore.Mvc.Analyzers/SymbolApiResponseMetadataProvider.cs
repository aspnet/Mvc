// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    internal class SymbolApiResponseMetadataProvider
    {
        internal static IList<ApiResponseMetadata> GetResponseMetadata(ApiControllerTypeCache typeCache, IMethodSymbol methodSymbol)
        {
            var responseMetadataAttributes = methodSymbol.GetAttributes(typeCache.IApiResponseMetadataProvider, inherit: true);
            var metadataItems = new List<ApiResponseMetadata>();
            foreach (var attribute in responseMetadataAttributes)
            {
                var statusCode = GetStatusCode(attribute);
                var metadata = new ApiResponseMetadata(statusCode, attribute, convention: null);

                metadataItems.Add(metadata);
            }

            return metadataItems;
        }

        private static int GetStatusCode(AttributeData attribute)
        {
            const int DefaultStatusCode = 200;

            for (var i = 0; i < attribute.NamedArguments.Length; i++)
            {
                var namedArgument = attribute.NamedArguments[i];
                var namedArgumentValue = namedArgument.Value;
                if (string.Equals(namedArgument.Key, "StatusCode", StringComparison.Ordinal) &&
                    namedArgumentValue.Kind == TypedConstantKind.Primitive &&
                    (namedArgumentValue.Type.SpecialType & SpecialType.System_Int32) == SpecialType.System_Int32 &&
                    namedArgumentValue.Value is int statusCode)
                {
                    return statusCode;
                }
            }

            if (attribute.AttributeConstructor == null)
            {
                return DefaultStatusCode;
            }

            var constructorParameters = attribute.AttributeConstructor.Parameters;
            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var parameter = constructorParameters[i];
                if (string.Equals(parameter.Name, "StatusCode", StringComparison.OrdinalIgnoreCase) &&
                    (parameter.Type.SpecialType & SpecialType.System_Int32) == SpecialType.System_Int32)
                {
                    if (attribute.ConstructorArguments.Length < i)
                    {
                        return DefaultStatusCode;
                    }

                    var argument = attribute.ConstructorArguments[i];
                    if (argument.Kind == TypedConstantKind.Primitive && argument.Value is int statusCode)
                    {
                        return statusCode;
                    }
                }
            }

            return DefaultStatusCode;
        }
    }

    internal readonly struct ApiResponseMetadata
    {
        public ApiResponseMetadata(int statusCode, AttributeData attributeData, IMethodSymbol convention)
        {
            StatusCode = statusCode;
            Attribute = attributeData;
            Convention = convention;
        }

        public int StatusCode { get; }

        public AttributeData Attribute { get; }

        public IMethodSymbol Convention { get; }
    }
}