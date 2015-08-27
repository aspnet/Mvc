﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    /// <summary>
    /// Utilities to work with creating <see cref="Attribute"/> instances from <see cref="AttributeData"/>.
    /// </summary>
    public static class CodeAnalysisAttributeUtilities
    {
        private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], Attribute>> _constructorCache =
            new ConcurrentDictionary<ConstructorInfo, Func<object[], Attribute>>();

        /// <summary>
        /// Gets the sequence of <see cref="Attribute"/>s of type <typeparamref name="TAttribute"/>
        /// that are declared on the specified <paramref name="symbol"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <param name="symbol">The <see cref="ISymbol"/> to find attributes on.</param>
        /// <param name="symbolLookup">The <see cref="CodeAnalysisSymbolLookupCache"/>.</param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            ISymbol symbol,
            CodeAnalysisSymbolLookupCache symbolLookup)
            where TAttribute : Attribute
        {
            var attributes = symbol.GetAttributes();
            if (attributes.Length > 0)
            {
                var attributeSymbol = symbolLookup.GetSymbol(typeof(TAttribute).GetTypeInfo());
                return attributes
                    .Where(attribute => attribute.AttributeClass == attributeSymbol)
                    .Select(attribute => CreateAttribute<TAttribute>(symbolLookup, attribute))
                    .ToArray();
            }

            return Enumerable.Empty<TAttribute>();
        }

        private static TAttribute CreateAttribute<TAttribute>(
            CodeAnalysisSymbolLookupCache symbolLookup,
            AttributeData attributeData)
            where TAttribute : Attribute
        {
            TAttribute attribute;
            if (attributeData.ConstructorArguments.Length == 0)
            {
                attribute = Activator.CreateInstance<TAttribute>();
            }
            else
            {
                var matchInfo = MatchConstructor(typeof(TAttribute), attributeData, symbolLookup);
                Func<object[], Attribute> constructorDelegate;
                if (!_constructorCache.TryGetValue(matchInfo.Constructor, out constructorDelegate))
                {
                    constructorDelegate = MakeFastConstructorInvoker(matchInfo);
                    _constructorCache[matchInfo.Constructor] = constructorDelegate;
                }

                attribute = (TAttribute)constructorDelegate(matchInfo.ArgumentValues);
            }

            if (attributeData.NamedArguments.Length > 0)
            {
                var helpers = PropertyHelper.GetVisibleProperties(attribute);
                foreach (var item in attributeData.NamedArguments)
                {
                    var helper = helpers.FirstOrDefault(
                        propertyHelper => string.Equals(propertyHelper.Name, item.Key, StringComparison.Ordinal));

                    if (helper == null)
                    {
                        throw new InvalidOperationException(
                            Resources.FormatCodeAnalysis_PropertyNotFound(item.Key, attribute.GetType().FullName));
                    }

                    var propertyValue = ConvertTypedConstantValue(
                        helper.Property.PropertyType, 
                        item.Value, 
                        symbolLookup);
                    helper.SetValue(attribute, propertyValue);
                }
            }

            return attribute;
        }

        private static Func<object[], Attribute> MakeFastConstructorInvoker(ConstructorMatchInfo matchInfo)
        {
            var argsParameter = Expression.Parameter(typeof(object[]), "args");
            var parameters = new Expression[matchInfo.ArgumentValues.Length];

            for (var index = 0; index < matchInfo.ArgumentValues.Length; index++)
            {
                parameters[index] =
                    Expression.Convert(
                        Expression.ArrayIndex(
                            argsParameter,
                            Expression.Constant(index)),
                        matchInfo.ArgumentValues[index].GetType());
            }


            // () => new TAttribute(args)
            var lambda =
                Expression.Lambda<Func<object[], Attribute>>(
                    Expression.New(
                        matchInfo.Constructor,
                        parameters),
                    argsParameter);

            return lambda.Compile();
        }

        private static ConstructorMatchInfo MatchConstructor(
            Type type,
            AttributeData attributeData,
            CodeAnalysisSymbolLookupCache symbolLookup)
        {
            var constructor = FindConstructor(type, attributeData.ConstructorArguments, symbolLookup);
            var constructorParmaters = constructor.GetParameters();

            var arguments = new object[attributeData.ConstructorArguments.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                var value = ConvertTypedConstantValue(
                    constructorParmaters[i].ParameterType,
                    attributeData.ConstructorArguments[i],
                    symbolLookup);

                arguments[i] = value;
            }

            return new ConstructorMatchInfo
            {
                Constructor = constructor,
                ArgumentValues = arguments
            };
        }

        private static ConstructorInfo FindConstructor(
            Type type,
            ImmutableArray<TypedConstant> symbolConstructorArguments,
            CodeAnalysisSymbolLookupCache symbolLookup)
        {
            var constructors = type.GetConstructors();
            // One or more constructor arguments were specified. Consequently,
            // we must have at least one constructor that matches.
            Debug.Assert(constructors.Length != 0);

            if (constructors.Length == 1)
            {
                return constructors[0];
            }

            foreach (var constructor in constructors)
            {
                var runtimeParameters = constructor.GetParameters();
                if (runtimeParameters.Length != symbolConstructorArguments.Length)
                {
                    continue;
                }

                var parametersMatched = true;
                for (var index = 0; index < runtimeParameters.Length; index++)
                {
                    var runtimeParameter = runtimeParameters[index].ParameterType;
                    if (symbolConstructorArguments[index].Kind == TypedConstantKind.Array &&
                        runtimeParameter.IsArray)
                    {
                        var arrayType = (IArrayTypeSymbol)symbolConstructorArguments[index].Type;
                        if (symbolLookup.GetSymbol(runtimeParameter.GetElementType().GetTypeInfo()) !=
                            arrayType.ElementType)
                        {
                            parametersMatched = false;
                            break;
                        }
                    }
                    else
                    {
                        var parameterSymbol = symbolLookup.GetSymbol(runtimeParameter.GetTypeInfo());
                        if (symbolConstructorArguments[index].Type != parameterSymbol)
                        {
                            parametersMatched = false;
                            break;
                        }
                    }
                }

                if (parametersMatched)
                {
                    return constructor;
                }
            }

            throw new InvalidOperationException(Resources.FormatCodeAnalysisConstructorNotFound(type.FullName));
        }

        private static object ConvertTypedConstantValue(
            Type type,
            TypedConstant constructorArgument,
            CodeAnalysisSymbolLookupCache symbolLookup)
        {
            object value;
            switch (constructorArgument.Kind)
            {
                case TypedConstantKind.Enum:
                    value = Enum.ToObject(type, constructorArgument.Value);
                    break;
                case TypedConstantKind.Primitive:
                    value = constructorArgument.Value;
                    break;
                case TypedConstantKind.Type:
                    var typeSymbol = (INamedTypeSymbol)constructorArgument.Value;
                    var typeName = CodeAnalysisSymbolBasedTypeInfo.GetAssemblyQualifiedName(typeSymbol);
                    value = Type.GetType(typeName);
                    break;
                case TypedConstantKind.Array:
                    Debug.Assert(type.IsArray && constructorArgument.Values != null);
                    var elementType = type.GetElementType();
                    var values = Array.CreateInstance(elementType, constructorArgument.Values.Length);
                    for (var index = 0; index < values.Length; index++)
                    {
                        values.SetValue(
                            ConvertTypedConstantValue(elementType, constructorArgument.Values[index], symbolLookup),
                            index);
                    }
                    value = values;
                    break;
                default:
                    throw new NotSupportedException(
                        Resources.FormatCodeAnalysis_TypeConstantKindNotSupported(constructorArgument.Kind));
            }

            return value;
        }

        private struct ConstructorMatchInfo
        {
            public ConstructorInfo Constructor;

            public object[] ArgumentValues;
        }
    }
}
