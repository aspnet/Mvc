// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// A visitor implementation that interprets <see cref="ValidationStateDictionary"/> to traverse
    /// a model object graph and perform validation.
    /// </summary>
    public class ValidationVisitor
    {
        private const string DataAnnotationsModelValidatorTypeName =
            "Microsoft.AspNetCore.Mvc.DataAnnotations.Internal.DataAnnotationsModelValidator";
        private const string RequiredAttributeTypeName =
            "System.ComponentModel.DataAnnotations.RequiredAttribute";
        private const string ValidatableObjectAdapterTypeName =
            "Microsoft.AspNetCore.Mvc.DataAnnotations.Internal.ValidatableObjectAdapter";
        private static MethodInfo DataAnnotationsModelValidatorAttributeGetter;
        private bool _isTopLevel;

        /// <summary>
        /// Creates a new <see cref="ValidationVisitor"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> associated with the current request.</param>
        /// <param name="validatorProvider">The <see cref="IModelValidatorProvider"/>.</param>
        /// <param name="validatorCache">The <see cref="ValidatorCache"/> that provides a list of <see cref="IModelValidator"/>s.</param>
        /// <param name="metadataProvider">The provider used for reading metadata for the model type.</param>
        /// <param name="validationState">The <see cref="ValidationStateDictionary"/>.</param>
        public ValidationVisitor(
            ActionContext actionContext,
            IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache,
            IModelMetadataProvider metadataProvider,
            ValidationStateDictionary validationState)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (validatorProvider == null)
            {
                throw new ArgumentNullException(nameof(validatorProvider));
            }

            if (validatorCache == null)
            {
                throw new ArgumentNullException(nameof(validatorCache));
            }

            Context = actionContext;
            ValidatorProvider = validatorProvider;
            Cache = validatorCache;

            MetadataProvider = metadataProvider;
            ValidationState = validationState;

            ModelState = actionContext.ModelState;
            CurrentPath = new ValidationStack();
        }

        protected IModelValidatorProvider ValidatorProvider { get; }
        protected IModelMetadataProvider MetadataProvider { get; }
        protected ValidatorCache Cache { get; }
        protected ActionContext Context { get; }
        protected ModelStateDictionary ModelState { get; }
        protected ValidationStateDictionary ValidationState { get; }
        protected ValidationStack CurrentPath { get; }

        protected object Container { get; set; }
        protected string Key { get; set; }
        protected object Model { get; set; }
        protected ModelMetadata Metadata { get; set; }
        protected IValidationStrategy Strategy { get; set; }

        /// <summary>
        /// Indicates whether validation of a complex type should be performed if validation fails for any of its children. The default behavior is false.
        /// </summary>
        public bool ValidateComplexTypesIfChildValidationFails { get; set; }

        /// <summary>
        /// Validates a object.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> associated with the model.</param>
        /// <param name="key">The model prefix key.</param>
        /// <param name="model">The model object.</param>
        /// <returns><c>true</c> if the object is valid, otherwise <c>false</c>.</returns>
        public bool Validate(ModelMetadata metadata, string key, object model)
        {
            return Validate(metadata, key, model, alwaysValidateAtTopLevel: false);
        }

        /// <summary>
        /// Validates a object.
        /// </summary>
        /// <param name="metadata">The <see cref="ModelMetadata"/> associated with the model.</param>
        /// <param name="key">The model prefix key.</param>
        /// <param name="model">The model object.</param>
        /// <param name="alwaysValidateAtTopLevel">If <c>true</c>, applies validation rules even if the top-level value is <c>null</c>.</param>
        /// <returns><c>true</c> if the object is valid, otherwise <c>false</c>.</returns>
        public virtual bool Validate(ModelMetadata metadata, string key, object model, bool alwaysValidateAtTopLevel)
        {
            if (model == null && key != null && !alwaysValidateAtTopLevel)
            {
                var entry = ModelState[key];
                if (entry != null && entry.ValidationState != ModelValidationState.Valid)
                {
                    entry.ValidationState = ModelValidationState.Valid;
                }

                return true;
            }

            _isTopLevel = true;

            return Visit(metadata, key, model);
        }

        /// <summary>
        /// Validates a single node in a model object graph.
        /// </summary>
        /// <returns><c>true</c> if the node is valid, otherwise <c>false</c>.</returns>
        protected virtual bool ValidateNode()
        {
            var state = ModelState.GetValidationState(Key);

            // Rationale: we might see the same model state key used for two different objects.
            // We want to run validation unless it's already known that this key is invalid.
            if (state != ModelValidationState.Invalid)
            {
                var validators = Cache.GetValidators(Metadata, ValidatorProvider);
                if (_isTopLevel && Model != null && Model.GetType() != Metadata.ModelType)
                {
                    validators = AddDerivedValidators(validators);
                }

                var count = validators.Count;
                if (count > 0)
                {
                    var context = new ModelValidationContext(
                        Context,
                        Metadata,
                        MetadataProvider,
                        Container,
                        Model);

                    var results = new List<ModelValidationResult>();
                    for (var i = 0; i < count; i++)
                    {
                        results.AddRange(validators[i].Validate(context));
                    }

                    var resultsCount = results.Count;
                    for (var i = 0; i < resultsCount; i++)
                    {
                        var result = results[i];
                        var key = ModelNames.CreatePropertyModelName(Key, result.MemberName);

                        // It's OK for key to be the empty string here. This can happen when a top
                        // level object implements IValidatableObject.
                        ModelState.TryAddModelError(key, result.Message);
                    }
                }
            }

            state = ModelState.GetFieldValidationState(Key);
            if (state == ModelValidationState.Invalid)
            {
                return false;
            }
            else
            {
                // If the field has an entry in ModelState, then record it as valid. Don't create
                // extra entries if they don't exist already.
                var entry = ModelState[Key];
                if (entry != null)
                {
                    entry.ValidationState = ModelValidationState.Valid;
                }

                return true;
            }
        }

        protected virtual bool Visit(ModelMetadata metadata, string key, object model)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (model != null && !CurrentPath.Push(model))
            {
                // This is a cycle, bail.
                return true;
            }

            var entry = GetValidationEntry(model);
            key = entry?.Key ?? key ?? string.Empty;
            metadata = entry?.Metadata ?? metadata;
            var strategy = entry?.Strategy;

            if (ModelState.HasReachedMaxErrors)
            {
                SuppressValidation(key);
                return false;
            }
            else if (entry != null && entry.SuppressValidation)
            {
                // Use the key on the entry, because we might not have entries in model state.
                SuppressValidation(entry.Key);
                CurrentPath.Pop(model);
                return true;
            }

            using (StateManager.Recurse(this, key ?? string.Empty, metadata, model, strategy))
            {
                if (Metadata.IsEnumerableType)
                {
                    return VisitComplexType(DefaultCollectionValidationStrategy.Instance);
                }

                if (Metadata.IsComplexType)
                {
                    return VisitComplexType(DefaultComplexObjectValidationStrategy.Instance);
                }

                return VisitSimpleType();
            }
        }

        // Covers everything VisitSimpleType does not i.e. both enumerations and complex types.
        protected virtual bool VisitComplexType(IValidationStrategy defaultStrategy)
        {
            var isValid = true;

            var savedMetadata = Metadata;
            var savedTopLevel = _isTopLevel;
            if (_isTopLevel && Model != null && !Metadata.IsEnumerableType && Model.GetType() != Metadata.ModelType)
            {
                // Handle properties of a derived type in the polymorphic binder case. (Derived collections can't have
                // metadata that overrides validation of their elements.)
                Metadata = MetadataProvider.GetMetadataForType(Model.GetType());
            }

            if (Model != null && Metadata.ValidateChildren)
            {
                var strategy = Strategy ?? defaultStrategy;
                _isTopLevel = false;

                isValid = VisitChildren(strategy);
            }
            else if (Model != null)
            {
                // Suppress validation for the entries matching this prefix. This will temporarily set
                // the current node to 'skipped' but we're going to visit it right away, so subsequent
                // code will set it to 'valid' or 'invalid'
                SuppressValidation(Key);
            }

            // Done validating children. Restore metadata because ValidateNode needs original information about the
            // parameter or property.
            _isTopLevel = savedTopLevel;
            Metadata = savedMetadata;

            // Double-checking HasReachedMaxErrors just in case this model has no properties.
            // If validation has failed for any children, only validate the parent if ValidateComplexTypesIfChildValidationFails is true.
            if ((isValid || ValidateComplexTypesIfChildValidationFails) && !ModelState.HasReachedMaxErrors)
            {
                isValid &= ValidateNode();
            }

            return isValid;
        }

        protected virtual bool VisitSimpleType()
        {
            if (ModelState.HasReachedMaxErrors)
            {
                SuppressValidation(Key);
                return false;
            }

            return ValidateNode();
        }

        protected virtual bool VisitChildren(IValidationStrategy strategy)
        {
            var isValid = true;
            var enumerator = strategy.GetChildren(Metadata, Key, Model);
            var parentEntry = new ValidationEntry(Metadata, Key, Model);

            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                var metadata = entry.Metadata;
                var key = entry.Key;
                if (metadata.PropertyValidationFilter?.ShouldValidateEntry(entry, parentEntry) == false)
                {
                    SuppressValidation(key);
                    continue;
                }

                isValid &= Visit(metadata, key, entry.Model);
            }

            return isValid;
        }

        protected virtual void SuppressValidation(string key)
        {
            if (key == null)
            {
                // If the key is null, that means that we shouldn't expect any entries in ModelState for
                // this value, so there's nothing to do.
                return;
            }

            var entries = ModelState.FindKeysWithPrefix(key);
            foreach (var entry in entries)
            {
                entry.Value.ValidationState = ModelValidationState.Skipped;
            }
        }

        protected virtual ValidationStateEntry GetValidationEntry(object model)
        {
            if (model == null || ValidationState == null)
            {
                return null;
            }

            ValidationState.TryGetValue(model, out var entry);
            return entry;
        }

        // Handle the derived type itself in the polymorphic model binder case. For example, derived type may implement
        // IValidatableObject though base type does not.
        //
        // Case not specific to IValidatableObject because an IValidationMetadataProvider could add a validator to type
        // metadata even if e.g the underlying ValidationAttribute cannot be associated with a class.
        private IReadOnlyList<IModelValidator> AddDerivedValidators(IReadOnlyList<IModelValidator> validators)
        {
            var derivedMetadata = MetadataProvider.GetMetadataForType(Model.GetType());
            var derivedValidators = Cache.GetValidators(derivedMetadata, ValidatorProvider);
            if (derivedValidators.Count == 0)
            {
                return validators;
            }

            if (validators.Count == 0)
            {
                // Simple case: The only validators are associated with the derived type.
                return derivedValidators;
            }

            if (derivedValidators.Count == 1 && IsValidatableObjectAdapter(derivedValidators[0]))
            {
                // If derived type implements IValidatableObject but base does not, add the adapter. Nothing to do if
                // validators collection already contains a ValidatableObjectAdapter.
                if (!validators.Any(v => IsValidatableObjectAdapter(v)))
                {
                    validators = validators
                        .Concat(derivedValidators)
                        .ToArray();
                }

                return validators;
            }

            // Likely an IValidationMetadataProvider implementation has associated validators with the derived type.
            // (May have also done so for the base type.)
            bool IsRequiredAttributeValidator(IModelValidator validator) =>
                IsDataAnnotationsModelValidator(validator) &&
                string.Equals(
                    RequiredAttributeTypeName,
                    GetValidationAttribute(validator).GetType().FullName,
                    StringComparison.Ordinal);

            // Ensure RequiredAttribute validator, if any, remains first.
            var firstValidator = validators[0];
            var firstDerivedValidator = derivedValidators[0];
            IEnumerable<IModelValidator> mergedValidators;
            if (IsRequiredAttributeValidator(firstValidator) &&
                IsRequiredAttributeValidator(firstDerivedValidator))
            {
                // RequiredAttribute likely associated with parameter or property. (Can't be positive because
                // ModelMetadata.IsRequired adds a unique RequiredAttribute to ModelMetadata.ValidatorMetadata.)
                mergedValidators = validators.Concat(derivedValidators.Skip(1));
            }
            else if (IsRequiredAttributeValidator(firstDerivedValidator))
            {
                // Move firstDerivedValidator to head of the list.
                mergedValidators = derivedValidators
                    .Take(1)
                    .Concat(validators)
                    .Concat(derivedValidators.Skip(1));
            }
            else
            {
                mergedValidators = validators.Concat(derivedValidators);
            }

            // Remove duplicate validators for the same attribute. This also removes duplicate ValidatableObjectAdapter
            // instances. But, may still get extra error messages from overridden ValidationAttribute instances or
            // duplicate non-ValidationAttribute validators.
            validators = mergedValidators
                .Distinct(ModelValidatorComparer.Instance)
                .ToArray();

            return validators;
        }

        private static object GetValidationAttribute(IModelValidator validator)
        {
            if (!IsDataAnnotationsModelValidator(validator))
            {
                return null;
            }

            if (DataAnnotationsModelValidatorAttributeGetter == null)
            {
                var propertyInfo = validator.GetType().GetProperty("Attribute");
                var methodInfo = propertyInfo.GetGetMethod();
                Interlocked.CompareExchange(ref DataAnnotationsModelValidatorAttributeGetter, methodInfo, null);
            }

            return DataAnnotationsModelValidatorAttributeGetter.Invoke(validator, Array.Empty<object>());
        }

        private static bool IsDataAnnotationsModelValidator(IModelValidator validator) => string.Equals(
            DataAnnotationsModelValidatorTypeName,
            validator.GetType().FullName,
            StringComparison.Ordinal);

        private static bool IsValidatableObjectAdapter(IModelValidator validator) => string.Equals(
            ValidatableObjectAdapterTypeName,
            validator.GetType().FullName,
            StringComparison.Ordinal);

        protected struct StateManager : IDisposable
        {
            private readonly ValidationVisitor _visitor;
            private readonly object _container;
            private readonly string _key;
            private readonly ModelMetadata _metadata;
            private readonly object _model;
            private readonly object _newModel;
            private readonly IValidationStrategy _strategy;

            public static StateManager Recurse(
                ValidationVisitor visitor,
                string key,
                ModelMetadata metadata,
                object model,
                IValidationStrategy strategy)
            {
                var recursifier = new StateManager(visitor, model);

                visitor.Container = visitor.Model;
                visitor.Key = key;
                visitor.Metadata = metadata;
                visitor.Model = model;
                visitor.Strategy = strategy;

                return recursifier;
            }

            public StateManager(ValidationVisitor visitor, object newModel)
            {
                _visitor = visitor;
                _newModel = newModel;

                _container = _visitor.Container;
                _key = _visitor.Key;
                _metadata = _visitor.Metadata;
                _model = _visitor.Model;
                _strategy = _visitor.Strategy;
            }

            public void Dispose()
            {
                _visitor.Container = _container;
                _visitor.Key = _key;
                _visitor.Metadata = _metadata;
                _visitor.Model = _model;
                _visitor.Strategy = _strategy;

                _visitor.CurrentPath.Pop(_newModel);
            }
        }

        private class ModelValidatorComparer : IEqualityComparer<IModelValidator>
        {
            public static readonly ModelValidatorComparer Instance = new ModelValidatorComparer();

            private ModelValidatorComparer()
            {
            }

            public bool Equals(IModelValidator x, IModelValidator y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                if (IsValidatableObjectAdapter(x))
                {
                    // All ValidatableObjectAdapter instances do the same thing.
                    return true;
                }

                if (!IsDataAnnotationsModelValidator(x))
                {
                    // Know nothing about semantics of other validators.
                    return false;
                }

                var xAttribute = GetValidationAttribute(x);
                var yAttribute = GetValidationAttribute(y);
                if (ReferenceEquals(xAttribute, yAttribute))
                {
                    return true;
                }

                if (xAttribute == null || yAttribute == null)
                {
                    return false;
                }

                // Attribute overrides Equals() to confirm internal states match.
                return xAttribute.Equals(yAttribute);
            }

            public int GetHashCode(IModelValidator obj)
            {
                if (IsValidatableObjectAdapter(obj))
                {
                    return 0;
                }

                if (!IsDataAnnotationsModelValidator(obj))
                {
                    return obj.GetHashCode();
                }

                var attribute = GetValidationAttribute(obj);

                return attribute.GetHashCode();
            }
        }
    }
}
