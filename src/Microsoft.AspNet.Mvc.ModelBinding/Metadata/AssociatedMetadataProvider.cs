// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class AssociatedMetadataProvider<TModelMetadata> : IModelMetadataProvider
        where TModelMetadata : ModelMetadata
    {
        private readonly ConcurrentDictionary<Type, TypeInformation> _typeInfoCache =
                new ConcurrentDictionary<Type, TypeInformation>();

        public IEnumerable<ModelMetadata> GetMetadataForProperties(object container, [NotNull] Type containerType)
        {
            return GetMetadataForPropertiesCore(container, containerType);
        }

        public ModelMetadata GetMetadataForProperty(Func<object> modelAccessor,
                                                    [NotNull] Type containerType,
                                                    [NotNull] string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "propertyName");
            }

            var typeInfo = GetTypeInformation(containerType);
            PropertyInformation propertyInfo;
            if (!typeInfo.Properties.TryGetValue(propertyName, out propertyInfo))
            {
                var message = Resources.FormatCommon_PropertyNotFound(containerType, propertyName);
                throw new ArgumentException(message, "propertyName");
            }

            return CreatePropertyMetadata(modelAccessor, propertyInfo);
        }

        public ModelMetadata GetMetadataForType(Func<object> modelAccessor, [NotNull] Type modelType)
        {
            var prototype = GetTypeInformation(modelType).Prototype;
            return CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        public ModelMetadata GetMetadataForParameter(
            Func<object> modelAccessor, 
            [NotNull] MethodInfo methodInfo, 
            [NotNull] string parameterName,
            IBinderMarker binderMarker)
        {
            var parameter = methodInfo.GetParameters().FirstOrDefault(
                param => StringComparer.Ordinal.Equals(param.Name, parameterName));
            if (parameter == null)
            {
                var message = Resources.FormatCommon_ParameterNotFound(parameterName);
                throw new ArgumentException(message, nameof(parameterName));
            }

            return GetMetadataForParameterCore(modelAccessor, parameterName, parameter, binderMarker);
        }

        // Override for creating the prototype metadata (without the accessor)
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes,
                                                                  Type containerType,
                                                                  Type modelType,
                                                                  string propertyName);

        // Override for applying the prototype + modelAccess to yield the final metadata
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype,
                                                                      Func<object> modelAccessor);
        private ModelMetadata GetMetadataForParameterCore(Func<object> modelAccessor,
                                                          string parameterName,
                                                          ParameterInfo parameter,
                                                          IBinderMarker binderMarker)
        {
            var parameterInfo = 
                CreateParameterInfo(parameter.ParameterType,
                                    parameter.GetCustomAttributes(),
                                    parameterName,
                                    binderMarker);

            var typeInfo = GetTypeInformation(parameter.ParameterType);
            UpdateMetadataWithTypeInfo(parameterInfo.Prototype, typeInfo);

            return CreateMetadataFromPrototype(parameterInfo.Prototype, modelAccessor);
        }

        private void UpdateMetadataWithTypeInfo(ModelMetadata parameterPrototype, TypeInformation typeInfo)
        {
            // If both are empty 
            //     Include everything.
            // If none are empty
            //     Include common. 
            // If nothing common
            //     Dont include anything.
            if (typeInfo.Prototype.IncludedProperties == null || typeInfo.Prototype.IncludedProperties.Count == 0)
            {
                if (parameterPrototype.IncludedProperties == null || parameterPrototype.IncludedProperties.Count == 0)
                {
                    parameterPrototype.IncludedProperties = typeInfo.Properties
                                                                    .Select(property => property.Key)
                                                                    .ToList();
                }
            }
            else
            {
                if (parameterPrototype.IncludedProperties == null || parameterPrototype.IncludedProperties.Count == 0)
                {
                    parameterPrototype.IncludedProperties = typeInfo.Prototype.IncludedProperties;
                }
                else
                {
                    parameterPrototype.IncludedProperties = parameterPrototype.IncludedProperties
                                                               .Intersect(typeInfo.Prototype.IncludedProperties,
                                                                       StringComparer.OrdinalIgnoreCase).ToList();
                }
            }

            if (typeInfo.Prototype.ExcludedProperties != null)
            {
                if (parameterPrototype.ExcludedProperties == null || parameterPrototype.ExcludedProperties.Count == 0)
                {
                    parameterPrototype.ExcludedProperties = typeInfo.Prototype.ExcludedProperties;
                }
                else
                {
                    parameterPrototype.ExcludedProperties = parameterPrototype.ExcludedProperties
                                                               .Union(typeInfo.Prototype.ExcludedProperties,
                                                                       StringComparer.OrdinalIgnoreCase).ToList();
                }
            }

            // Ignore the ModelName specified at Type level. (This is to be compatible with MVC).
        }

        private IEnumerable<ModelMetadata> GetMetadataForPropertiesCore(object container, Type containerType)
        {
            var typeInfo = GetTypeInformation(containerType);
            foreach (var kvp in typeInfo.Properties)
            {
                var propertyInfo = kvp.Value;
                Func<object> modelAccessor = null;
                if (container != null)
                {
                    modelAccessor = () => propertyInfo.PropertyHelper.GetValue(container);
                }
                yield return CreatePropertyMetadata(modelAccessor, propertyInfo);
            }
        }

        private TModelMetadata CreatePropertyMetadata(Func<object> modelAccessor, PropertyInformation propertyInfo)
        {
            var metadata = CreateMetadataFromPrototype(propertyInfo.Prototype, modelAccessor);
            if (propertyInfo.IsReadOnly)
            {
                metadata.IsReadOnly = true;
            }

            return metadata;
        }

        private TypeInformation GetTypeInformation(Type type, IEnumerable<Attribute> associatedAttributes = null)
        {
            // This retrieval is implemented as a TryGetValue/TryAdd instead of a GetOrAdd 
            // to avoid the performance cost of creating instance delegates
            TypeInformation typeInfo;
            if (!_typeInfoCache.TryGetValue(type, out typeInfo))
            {
                typeInfo = CreateTypeInformation(type, associatedAttributes);
                _typeInfoCache.TryAdd(type, typeInfo);
            }
            return typeInfo;
        }

        private TypeInformation CreateTypeInformation(Type type, IEnumerable<Attribute> associatedAttributes)
        {
            var typeInfo = type.GetTypeInfo();
            var attributes = typeInfo.GetCustomAttributes();
            if (associatedAttributes != null)
            {
                attributes = attributes.Concat(associatedAttributes);
            }
            var info = new TypeInformation
            {
                Prototype = CreateMetadataPrototype(attributes,
                                                    containerType: null,
                                                    modelType: type,
                                                    propertyName: null)
            };

            var properties = new Dictionary<string, PropertyInformation>(StringComparer.Ordinal);
            foreach (var propertyHelper in PropertyHelper.GetProperties(type))
            {
                // Avoid re-generating a property descriptor if one has already been generated for the property name
                if (!properties.ContainsKey(propertyHelper.Name))
                {
                    properties.Add(propertyHelper.Name, CreatePropertyInformation(type, propertyHelper));
                }
            }

            info.Properties = properties;

            if (info.Prototype != null)
            {
                // Update the included properties so that the properties are not ignored while binding.
                if (info.Prototype.IncludedProperties == null ||
                    info.Prototype.IncludedProperties.Count == 0)
                {
                    // Mark all properties as included.
                    info.Prototype.IncludedProperties =
                        info.Properties.Select(property => property.Key).ToList();
                }
            }

            return info;
        }

        private PropertyInformation CreatePropertyInformation(Type containerType, PropertyHelper helper)
        {
            var property = helper.Property;
            return new PropertyInformation
            {
                PropertyHelper = helper,
                Prototype = CreateMetadataPrototype(property.GetCustomAttributes(),
                                                    containerType,
                                                    property.PropertyType,
                                                    property.Name),
                IsReadOnly = !property.CanWrite || property.SetMethod.IsPrivate
            };
        }

        private ParameterInformation CreateParameterInfo(
            Type parameterType, 
            IEnumerable<Attribute> attributes, 
            string parameterName,
            IBinderMarker binderMarker)
        {
            var metadataProtoType = CreateMetadataPrototype(attributes: attributes,
                                                    containerType: null,
                                                    modelType: parameterType,
                                                    propertyName: parameterName);
      
            if (binderMarker != null)
            {
                metadataProtoType.Marker = binderMarker;
            }

            var nameProvider = binderMarker as IModelNameProvider;
            if (nameProvider != null && nameProvider.Name != null)
            {
                metadataProtoType.ModelName = nameProvider.Name;
            }

            return new ParameterInformation
            {
                Prototype =  metadataProtoType
            };
        }

        private sealed class ParameterInformation
        {
            public TModelMetadata Prototype { get; set; }
        }

        private sealed class TypeInformation
        {
            public TModelMetadata Prototype { get; set; }
            public Dictionary<string, PropertyInformation> Properties { get; set; }
        }

        private sealed class PropertyInformation
        {
            public PropertyHelper PropertyHelper { get; set; }
            public TModelMetadata Prototype { get; set; }
            public bool IsReadOnly { get; set; }
        }
    }
}
