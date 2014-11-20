// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class MutableObjectModelBinder : IModelBinder
    {
        public virtual async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (!CanBindType(bindingContext.ModelType))
            {
                return false;
            }

            var topLevelObject = bindingContext.ModelMetadata.ContainerType == null;
            var isThereAnExplicitAlias = bindingContext.ModelMetadata.ModelName != null;

            
            // The first check is necessary because if we fallback to empty prefix, we do not want to depend
            // on a value provider to provide a value for empty prefix.
            var containsPrefix = (bindingContext.ModelName == string.Empty && topLevelObject) ||
                                 await bindingContext.ValueProvider.ContainsPrefixAsync(bindingContext.ModelName);

            // Always create the model if
            // 1. It is a top level object and the model name is empty.
            // 2. There is a value provider which can provide value for the model name.
            // 3. There is an explicit alias provided by the user and it is a top level object.
            // The reson we depend on explicit alias is that otherwise we want the FallToEmptyPrefix codepath
            // to kick in so that empty prefix values could be bound.
            if (!containsPrefix && !(isThereAnExplicitAlias && topLevelObject))
            {
                return false;
            }

            EnsureModel(bindingContext);
            var propertyMetadatas = GetMetadataForProperties(bindingContext).ToArray();
            var dto = CreateAndPopulateDto(bindingContext, propertyMetadatas);

            // post-processing, e.g. property setters and hooking up validation
            ProcessDto(bindingContext, dto);
            // complex models require full validation
            bindingContext.ValidationNode.ValidateAllProperties = true;
            return true;
        }

        protected virtual bool CanUpdateProperty(ModelMetadata propertyMetadata)
        {
            return CanUpdatePropertyInternal(propertyMetadata);
        }

        private static bool CanBindType(Type modelType)
        {
            // Simple types cannot use this binder
            var isComplexType = !TypeHelper.HasStringConverter(modelType);
            if (!isComplexType)
            {
                return false;
            }

            if (modelType == typeof(ComplexModelDto))
            {
                // forbidden type - will cause a stack overflow if we try binding this type
                return false;
            }
            return true;
        }

        internal static bool CanUpdatePropertyInternal(ModelMetadata propertyMetadata)
        {
            return !propertyMetadata.IsReadOnly || CanUpdateReadOnlyProperty(propertyMetadata.ModelType);
        }

        private static bool CanUpdateReadOnlyProperty(Type propertyType)
        {
            // Value types have copy-by-value semantics, which prevents us from updating
            // properties that are marked readonly.
            if (propertyType.GetTypeInfo().IsValueType)
            {
                return false;
            }

            // Arrays are strange beasts since their contents are mutable but their sizes aren't.
            // Therefore we shouldn't even try to update these. Further reading:
            // http://blogs.msdn.com/ericlippert/archive/2008/09/22/arrays-considered-somewhat-harmful.aspx
            if (propertyType.IsArray)
            {
                return false;
            }

            // Special-case known immutable reference types
            if (propertyType == typeof(string))
            {
                return false;
            }

            return true;
        }

        private ComplexModelDto CreateAndPopulateDto(ModelBindingContext bindingContext,
                                                     IEnumerable<ModelMetadata> propertyMetadatas)
        {
            // create a DTO and call into the DTO binder
            var originalDto = new ComplexModelDto(bindingContext.ModelMetadata, propertyMetadatas);
            var dtoBindingContext = new ModelBindingContext(bindingContext)
            {
                ModelMetadata = bindingContext.MetadataProvider.GetMetadataForType(() => originalDto,
                                                                                   typeof(ComplexModelDto)),
                ModelName = bindingContext.ModelName
            };

            bindingContext.ModelBinder.BindModelAsync(dtoBindingContext);
            return (ComplexModelDto)dtoBindingContext.Model;
        }

        protected virtual object CreateModel(ModelBindingContext bindingContext)
        {
            // If the Activator throws an exception, we want to propagate it back up the call stack, since the 
            // application developer should know that this was an invalid type to try to bind to.
            return Activator.CreateInstance(bindingContext.ModelType);
        }

        // Called when the property setter null check failed, allows us to add our own error message to ModelState.
        internal static EventHandler<ModelValidatedEventArgs> CreateNullCheckFailedHandler(ModelMetadata modelMetadata,
                                                                                           object incomingValue)
        {
            return (sender, e) =>
            {
                var validationNode = (ModelValidationNode)sender;
                var modelState = e.ValidationContext.ModelState;
                var validationState = modelState.GetFieldValidationState(validationNode.ModelStateKey);

                if (validationState == ModelValidationState.Unvalidated)
                {
                    // TODO: #450 Revive ModelBinderConfig
                    // var errorMessage =  ModelBinderConfig.ValueRequiredErrorMessageProvider(e.ValidationContext, 
                    //                                                                            modelMetadata, 
                    //                                                                            incomingValue);
                    var errorMessage = Resources.ModelBinderConfig_ValueRequired;
                    if (errorMessage != null)
                    {
                        modelState.TryAddModelError(validationNode.ModelStateKey, errorMessage);
                    }
                }
            };
        }

        protected virtual void EnsureModel(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model == null)
            {
                bindingContext.ModelMetadata.Model = CreateModel(bindingContext);
            }
        }

        protected virtual IEnumerable<ModelMetadata> GetMetadataForProperties(ModelBindingContext bindingContext)
        {
            var validationInfo = GetPropertyValidationInfo(bindingContext);
            var propertyTypeMetadata = bindingContext.MetadataProvider
                                                       .GetMetadataForType(null, bindingContext.ModelType);
            Predicate<string> newPropertyFilter =
                propertyName => bindingContext.PropertyFilter(propertyName) &&
                                BindAttribute.IsPropertyAllowed(
                                                propertyName,
                                                propertyTypeMetadata.IncludedProperties,
                                                propertyTypeMetadata.ExcludedProperties);

            return bindingContext.ModelMetadata.Properties
                                 .Where(propertyMetadata =>
                                    newPropertyFilter(propertyMetadata.PropertyName) &&
                                    (validationInfo.RequiredProperties.Contains(propertyMetadata.PropertyName) ||
                                    !validationInfo.SkipProperties.Contains(propertyMetadata.PropertyName)) &&
                                    CanUpdateProperty(propertyMetadata));
        }

        private static object GetPropertyDefaultValue(PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
            return (attr != null) ? attr.Value : null;
        }

        internal static PropertyValidationInfo GetPropertyValidationInfo(ModelBindingContext bindingContext)
        {
            var validationInfo = new PropertyValidationInfo();
            var modelTypeInfo = bindingContext.ModelType.GetTypeInfo();
            var typeAttribute = modelTypeInfo.GetCustomAttribute<BindingBehaviorAttribute>();
            var properties = bindingContext.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyMetadata = bindingContext.PropertyMetadata[propertyName];
                var requiredValidator = bindingContext.ValidatorProvider
                                                      .GetValidators(propertyMetadata)
                                                      .FirstOrDefault(v => v != null && v.IsRequired);
                if (requiredValidator != null)
                {
                    validationInfo.RequiredValidators[propertyName] = requiredValidator;
                }

                var propertyAttribute = property.GetCustomAttribute<BindingBehaviorAttribute>();
                var bindingBehaviorAttribute = propertyAttribute ?? typeAttribute;
                if (bindingBehaviorAttribute != null)
                {
                    switch (bindingBehaviorAttribute.Behavior)
                    {
                        case BindingBehavior.Required:
                            validationInfo.RequiredProperties.Add(propertyName);
                            break;

                        case BindingBehavior.Never:
                            validationInfo.SkipProperties.Add(propertyName);
                            break;
                    }
                }
                else if (requiredValidator != null)
                {
                    validationInfo.RequiredProperties.Add(propertyName);
                }
            }

            return validationInfo;
        }

        internal void ProcessDto(ModelBindingContext bindingContext, ComplexModelDto dto)
        {
            var validationInfo = GetPropertyValidationInfo(bindingContext);

            // Eliminate provided properties from requiredProperties; leaving just *missing* required properties.
            validationInfo.RequiredProperties.ExceptWith(dto.Results.Select(r => r.Key.PropertyName));

            foreach (var missingRequiredProperty in validationInfo.RequiredProperties)
            {
                var addedError = false;
                var modelStateKey = ModelBindingHelper.CreatePropertyModelName(
                    bindingContext.ValidationNode.ModelStateKey, missingRequiredProperty);

                // Update Model as SetProperty() would: Place null value where validator will check for non-null. This
                // ensures a failure result from a required validator (if any) even for a non-nullable property.
                // (Otherwise, propertyMetadata.Model is likely already null.)
                var propertyMetadata = bindingContext.PropertyMetadata[missingRequiredProperty];
                propertyMetadata.Model = null;

                // Execute validator (if any) to get custom error message.
                IModelValidator validator;
                if (validationInfo.RequiredValidators.TryGetValue(missingRequiredProperty, out validator))
                {
                    addedError = RunValidator(validator, bindingContext, propertyMetadata, modelStateKey);
                }

                // Fall back to default message if BindingBehaviorAttribute required this property or validator
                // (oddly) succeeded.
                if (!addedError)
                {
                    bindingContext.ModelState.TryAddModelError(
                        modelStateKey,
                        Resources.FormatMissingRequiredMember(missingRequiredProperty));
                }
            }

            // for each property that was bound, call the setter, recording exceptions as necessary
            foreach (var entry in dto.Results)
            {
                var propertyMetadata = entry.Key;
                var dtoResult = entry.Value;
                if (dtoResult != null)
                {
                    IModelValidator requiredValidator;
                    validationInfo.RequiredValidators.TryGetValue(propertyMetadata.PropertyName,
                                                                  out requiredValidator);
                    SetProperty(bindingContext, propertyMetadata, dtoResult, requiredValidator);
                    bindingContext.ValidationNode.ChildNodes.Add(dtoResult.ValidationNode);
                }
            }
        }

        protected virtual void SetProperty(ModelBindingContext bindingContext,
                                           ModelMetadata propertyMetadata,
                                           ComplexModelDtoResult dtoResult,
                                           IModelValidator requiredValidator)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            var property = bindingContext.ModelType
                                         .GetProperty(propertyMetadata.PropertyName, bindingFlags);

            if (property == null || !property.CanWrite)
            {
                // nothing to do
                return;
            }

            var value = dtoResult.Model ?? GetPropertyDefaultValue(property);
            propertyMetadata.Model = value;

            // 'Required' validators need to run first so that we can provide useful error messages if
            // the property setters throw, e.g. if we're setting entity keys to null. 
            if (value == null)
            {
                var modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                var validationState = bindingContext.ModelState.GetFieldValidationState(modelStateKey);
                if (validationState == ModelValidationState.Unvalidated)
                {
                    if (requiredValidator != null)
                    {
                        var validationContext = new ModelValidationContext(bindingContext, propertyMetadata);
                        foreach (var validationResult in requiredValidator.Validate(validationContext))
                        {
                            bindingContext.ModelState.TryAddModelError(modelStateKey, validationResult.Message);
                        }
                    }
                }
            }

            if (value != null || property.PropertyType.AllowsNullValue())
            {
                try
                {
                    property.SetValue(bindingContext.Model, value);
                }
                catch (Exception ex)
                {
                    // don't display a duplicate error message if a binding error has already occurred for this field
                    var targetInvocationException = ex as TargetInvocationException;
                    if (targetInvocationException != null &&
                        targetInvocationException.InnerException != null)
                    {
                        ex = targetInvocationException.InnerException;
                    }
                    var modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                    var validationState = bindingContext.ModelState.GetFieldValidationState(modelStateKey);
                    if (validationState == ModelValidationState.Unvalidated)
                    {
                        bindingContext.ModelState.AddModelError(modelStateKey, ex);
                    }
                }
            }
            else
            {
                // trying to set a non-nullable value type to null, need to make sure there's a message
                var modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                var validationState = bindingContext.ModelState.GetFieldValidationState(modelStateKey);
                if (validationState == ModelValidationState.Unvalidated)
                {
                    dtoResult.ValidationNode.Validated += CreateNullCheckFailedHandler(propertyMetadata, value);
                }
            }
        }

        // Returns true if validator execution adds a model error.
        private static bool RunValidator(IModelValidator validator,
                                         ModelBindingContext bindingContext,
                                         ModelMetadata propertyMetadata,
                                         string modelStateKey)
        {
            var validationContext = new ModelValidationContext(bindingContext, propertyMetadata);

            var addedError = false;
            foreach (var validationResult in validator.Validate(validationContext))
            {
                bindingContext.ModelState.TryAddModelError(modelStateKey, validationResult.Message);
                addedError = true;
            }

            if (!addedError)
            {
                bindingContext.ModelState.MarkFieldValid(modelStateKey);
            }

            return addedError;
        }

        private static bool IsPropertyAllowed(string propertyName,
                                              IReadOnlyList<string> includeProperties,
                                              IReadOnlyList<string> excludeProperties)
        {
            // We allow a property to be bound if its both in the include list AND not in the exclude list.
            // An empty exclude list implies no properties are disallowed.
            var includeProperty = (includeProperties != null) &&
                                  includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);

            var excludeProperty = (excludeProperties != null) &&
                                  excludeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);

            return includeProperty && !excludeProperty;
        }

        internal sealed class PropertyValidationInfo
        {
            public PropertyValidationInfo()
            {
                RequiredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                RequiredValidators = new Dictionary<string, IModelValidator>(StringComparer.OrdinalIgnoreCase);
                SkipProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public HashSet<string> RequiredProperties { get; private set; }

            public Dictionary<string, IModelValidator> RequiredValidators { get; private set; }

            public HashSet<string> SkipProperties { get; private set; }
        }
    }
}
