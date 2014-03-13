﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class MutableObjectModelBinder : IModelBinder
    {
        public virtual bool BindModel(ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (!CanBindType(bindingContext.ModelType) ||
                !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                return false;
            }

            EnsureModel(bindingContext);
            var propertyMetadatas = GetMetadataForProperties(bindingContext);
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
            var isComplexType = !modelType.HasStringConverter();
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

        private ComplexModelDto CreateAndPopulateDto(ModelBindingContext bindingContext, IEnumerable<ModelMetadata> propertyMetadatas)
        {
            // create a DTO and call into the DTO binder
            var originalDto = new ComplexModelDto(bindingContext.ModelMetadata, propertyMetadatas);
            var dtoBindingContext = new ModelBindingContext(bindingContext)
            {
                ModelMetadata = bindingContext.MetadataProvider.GetMetadataForType(() => originalDto, typeof(ComplexModelDto)),
                ModelName = bindingContext.ModelName
            };

            bindingContext.ModelBinder.BindModel(dtoBindingContext);
            return (ComplexModelDto)dtoBindingContext.Model;
        }

        protected virtual object CreateModel(ModelBindingContext bindingContext)
        {
            // If the Activator throws an exception, we want to propagate it back up the call stack, since the application
            // developer should know that this was an invalid type to try to bind to.
            return Activator.CreateInstance(bindingContext.ModelType);
        }

        // Called when the property setter null check failed, allows us to add our own error message to ModelState.
        internal static EventHandler<ModelValidatedEventArgs> CreateNullCheckFailedHandler(ModelMetadata modelMetadata, object incomingValue)
        {
            return (sender, e) =>
            {
                var validationNode = (ModelValidationNode)sender;
                var modelState = e.ValidationContext.ModelState;

                if (modelState.IsValidField(validationNode.ModelStateKey))
                {
                    // TODO: Revive ModelBinderConfig
                    // string errorMessage =  ModelBinderConfig.ValueRequiredErrorMessageProvider(e.ValidationContext, modelMetadata, incomingValue);
                    var errorMessage = e.ValidationContext.ModelMetadata.PropertyName + " is required";
                    if (errorMessage != null)
                    {
                        modelState.AddModelError(validationNode.ModelStateKey, errorMessage);
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
            // keep a set of the required properties so that we can cross-reference bound properties later
            HashSet<string> requiredProperties;
            Dictionary<string, IModelValidator> requiredValidators;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext, out requiredProperties, out requiredValidators, out skipProperties);

            return from propertyMetadata in bindingContext.ModelMetadata.Properties
                   let propertyName = propertyMetadata.PropertyName
                   let shouldUpdateProperty = requiredProperties.Contains(propertyName) || !skipProperties.Contains(propertyName)
                   where shouldUpdateProperty && CanUpdateProperty(propertyMetadata)
                   select propertyMetadata;
        }

        private static object GetPropertyDefaultValue(PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
            return (attr != null) ? attr.Value : null;
        }

        internal static void GetRequiredPropertiesCollection(ModelBindingContext bindingContext,
                                                            out HashSet<string> requiredProperties,
                                                            out Dictionary<string, IModelValidator> requiredValidators,
                                                            out HashSet<string> skipProperties)
        {
            requiredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            requiredValidators = new Dictionary<string, IModelValidator>(StringComparer.OrdinalIgnoreCase);
            skipProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // TODO: HttpBindingBehaviorAttribute
            var modelTypeInfo = bindingContext.ModelType.GetTypeInfo();
            foreach (var propertyMetadata in bindingContext.ModelMetadata.Properties)
            {
                var propertyName = propertyMetadata.PropertyName;
                var requiredValidator = bindingContext.GetValidators(propertyMetadata)
                                                      .FirstOrDefault(v => v.IsRequired);
                // TODO: Revive HttpBindingBehaviorAttribute

                if (requiredValidator != null)
                {
                    requiredValidators[propertyName] = requiredValidator;
                    requiredProperties.Add(propertyName);
                }
            }
        }

        internal void ProcessDto(ModelBindingContext bindingContext, ComplexModelDto dto)
        {
            HashSet<string> requiredProperties;
            Dictionary<string, IModelValidator> requiredValidators;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext, out requiredProperties, out requiredValidators, out skipProperties);

            // Eliminate provided properties from requiredProperties; leaving just *missing* required properties.
            requiredProperties.ExceptWith(dto.Results.Select(r => r.Key.PropertyName));

            foreach (var missingRequiredProperty in requiredProperties)
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
                if (requiredValidators.TryGetValue(missingRequiredProperty, out validator))
                {
                    addedError = RunValidator(validator, bindingContext, propertyMetadata, modelStateKey);
                }

                // Fall back to default message if HttpBindingBehaviorAttribute required this property or validator
                // (oddly) succeeded.
                if (!addedError)
                {
                    bindingContext.ModelState.AddModelError(
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
                    SetProperty(bindingContext, propertyMetadata, dtoResult);
                    bindingContext.ValidationNode.ChildNodes.Add(dtoResult.ValidationNode);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We're recording this exception so that we can act on it later.")]
        protected virtual void SetProperty(ModelBindingContext bindingContext,
                                           ModelMetadata propertyMetadata,
                                           ComplexModelDtoResult dtoResult)
        {
            var property = bindingContext.ModelType
                                         .GetProperty(propertyMetadata.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

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
                if (bindingContext.ModelState.IsValidField(modelStateKey))
                {
                    var requiredValidator = bindingContext.GetValidators(propertyMetadata).FirstOrDefault(v => v.IsRequired);
                    if (requiredValidator != null)
                    {
                        var validationContext = bindingContext.CreateValidationContext(propertyMetadata);
                        foreach (var validationResult in requiredValidator.Validate(validationContext))
                        {
                            bindingContext.ModelState.AddModelError(modelStateKey, validationResult.Message);
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
                    var modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                    if (bindingContext.ModelState.IsValidField(modelStateKey))
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                    }
                }
            }
            else
            {
                // trying to set a non-nullable value type to null, need to make sure there's a message
                var modelStateKey = dtoResult.ValidationNode.ModelStateKey;
                if (bindingContext.ModelState.IsValidField(modelStateKey))
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
            var validationContext = bindingContext.CreateValidationContext(propertyMetadata);

            var addedError = false;
            foreach (var validationResult in validator.Validate(validationContext))
            {
                bindingContext.ModelState.AddModelError(modelStateKey, validationResult.Message);
                addedError = true;
            }
            return addedError;
        }
    }
}
