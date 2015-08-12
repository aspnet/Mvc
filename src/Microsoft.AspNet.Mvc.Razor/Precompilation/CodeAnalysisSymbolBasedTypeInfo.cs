// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    /// <summary>
    /// <see cref="ITypeInfo"/> implementation using Code Analysis symbols.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class CodeAnalysisSymbolBasedTypeInfo : ITypeInfo
    {
        private static readonly System.Reflection.TypeInfo OpenGenericDictionaryTypeInfo =
            typeof(IDictionary<,>).GetTypeInfo();
        private readonly CodeAnalysisSymbolLookupCache _symbolLookup;
        private readonly ITypeSymbol _type;
        private string _fullName;
        private List<IPropertyInfo> _properties;

        /// <summary>
        /// Initializes a new instance of <see cref="CodeAnalysisSymbolBasedTypeInfo"/>.
        /// </summary>
        /// <param name="propertySymbol">The <see cref="IPropertySymbol"/>.</param>
        /// <param name="symbolLookup">The <see cref="CodeAnalysisSymbolLookupCache"/>.</param>
        public CodeAnalysisSymbolBasedTypeInfo(
            [NotNull] ITypeSymbol type,
            [NotNull] CodeAnalysisSymbolLookupCache symbolLookup)
        {
            _symbolLookup = symbolLookup;
            _type = type;
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if (_fullName == null)
                {
                    _fullName = GetFullName(_type);
                }

                return _fullName;
            }
        }

        /// <inheritdoc />
        public bool IsAbstract => _type.IsAbstract;

        /// <inheritdoc />
        public bool IsGenericType
        {
            get
            {
                return _type.Kind == SymbolKind.NamedType &&
                    ((INamedTypeSymbol)_type).IsGenericType;
            }
        }

        /// <inheritdoc />
        public bool IsNested => _type.ContainingType != null;

        /// <inheritdoc />
        public bool IsPublic => _type.DeclaredAccessibility == Accessibility.Public;

        /// <inheritdoc />
        public string Name => _type.MetadataName;

        /// <inheritdoc />
        public IEnumerable<IPropertyInfo> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = PopulateProperties();
                }

                return _properties;
            }
        }

        /// <inheritdoc />
        public IEnumerable<TAttribute> GetCustomAttributes<TAttribute>()
            where TAttribute : Attribute
        {
            return CodeAnalysisAttributeUtilities.GetCustomAttributes<TAttribute>(_type, _symbolLookup);
        }

        /// <inheritdoc />
        public bool ImplementsInterface(System.Reflection.TypeInfo interfaceType)
        {
            var interfaceSymbol = _symbolLookup.GetSymbol(interfaceType);
            return _type.AllInterfaces.Any(implementedInterface => implementedInterface == interfaceSymbol);
        }

        /// <inheritdoc />
        public string[] GetGenericDictionaryParameterNames()
        {
            var dictionarySymbol = _symbolLookup.GetSymbol(OpenGenericDictionaryTypeInfo);

            INamedTypeSymbol dictionaryInterface;
            if (_type.Kind == SymbolKind.NamedType &&
                ((INamedTypeSymbol)_type).ConstructedFrom == dictionarySymbol)
            {
                dictionaryInterface = (INamedTypeSymbol)_type;
            }
            else
            {
                dictionaryInterface = _type
                    .AllInterfaces
                    .FirstOrDefault(implementedInterface => implementedInterface.ConstructedFrom == dictionarySymbol);
            }

            if (dictionaryInterface != null)
            {
                Debug.Assert(dictionaryInterface.TypeArguments.Length == 2);

                return new[]
                {
                    GetFullName(dictionaryInterface.TypeArguments[0]),
                    GetFullName(dictionaryInterface.TypeArguments[1])
                };
            }

            return null;
        }

        /// <summary>
        /// Gets the assembly qualified named of the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The <see cref="ITypeSymbol" /> to generate the name for.</param>
        /// <returns>The assembly qualified name.</returns>
        public static string GetAssemblyQualifiedName([NotNull] ITypeSymbol symbol)
        {
            var builder = new StringBuilder();
            GetAssemblyQualifiedName(builder, symbol);

            return builder.ToString();
        }

        private List<IPropertyInfo> PopulateProperties()
        {
            var properties = new List<IPropertyInfo>();
            var overridenProperties = new HashSet<IPropertySymbol>();
            var type = _type;

            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member.Kind == SymbolKind.Property)
                    {
                        var propertySymbol = (IPropertySymbol)member;
                        if (!propertySymbol.IsIndexer && !overridenProperties.Contains(propertySymbol))
                        {
                            var propertyInfo = new CodeAnalysisSymbolBasedPropertyInfo(propertySymbol, _symbolLookup);
                            properties.Add(propertyInfo);

                            if (propertySymbol.IsOverride)
                            {
                                overridenProperties.Add(propertySymbol.OverriddenProperty);
                            }
                        }
                    }
                }

                type = type.BaseType;
            }

            return properties;
        }

        private static string GetFullName(ITypeSymbol typeSymbol)
        {
            var nameBuilder = new StringBuilder();
            GetFullName(nameBuilder, typeSymbol);

            return nameBuilder.Length == 0 ? null : nameBuilder.ToString();
        }

        private static void GetFullName(StringBuilder nameBuilder, ITypeSymbol typeSymbol)
        {
            if (typeSymbol.Kind == SymbolKind.TypeParameter)
            {
                return;
            }

            var insertIndex = nameBuilder.Length;
            nameBuilder.Append(typeSymbol.MetadataName);
            if (typeSymbol.Kind == SymbolKind.NamedType)
            {
                var namedSymbol = (INamedTypeSymbol)typeSymbol;
                // The symbol represents a generic but not open generic type
                if (namedSymbol.IsGenericType &&
                    namedSymbol.ConstructedFrom != namedSymbol)
                {
                    nameBuilder.Append('[');
                    foreach (var typeArgument in namedSymbol.TypeArguments)
                    {
                        nameBuilder.Append('[');
                        GetAssemblyQualifiedName(nameBuilder, typeArgument);
                        nameBuilder
                            .Append(']')
                            .Append(',');
                    }

                    nameBuilder.Length--;
                    nameBuilder.Append("]");
                }
            }

            var containingType = typeSymbol.ContainingType;
            while (containingType != null)
            {
                nameBuilder
                    .Insert(insertIndex, '+')
                    .Insert(insertIndex, containingType.MetadataName);

                containingType = containingType.ContainingType;
            }

            var containingNamespace = typeSymbol.ContainingNamespace;
            while (!containingNamespace.IsGlobalNamespace)
            {
                nameBuilder
                    .Insert(insertIndex, '.')
                    .Insert(insertIndex, containingNamespace.MetadataName);

                containingNamespace = containingNamespace.ContainingNamespace;
            }
        }

        private static void GetAssemblyQualifiedName(StringBuilder builder, ITypeSymbol typeSymbol)
        {
            GetFullName(builder, typeSymbol);
            builder
                .Append(", ")
                .Append(typeSymbol.ContainingAssembly.Identity);
        }
    }
}