﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationNode
    {
        private readonly List<ModelValidationNode> _childNodes;

        public ModelValidationNode(ModelMetadata modelMetadata, string modelStateKey)
            : this(modelMetadata, modelStateKey, null)
        {
        }

        public ModelValidationNode([NotNull] ModelMetadata modelMetadata, 
                                   [NotNull] string modelStateKey, 
                                   IEnumerable<ModelValidationNode> childNodes)
        {
            ModelMetadata = modelMetadata;
            ModelStateKey = modelStateKey;
            _childNodes = (childNodes != null) ? childNodes.ToList() : new List<ModelValidationNode>();
        }

        public event EventHandler<ModelValidatedEventArgs> Validated;

        public event EventHandler<ModelValidatingEventArgs> Validating;

        public ICollection<ModelValidationNode> ChildNodes
        {
            get { return _childNodes; }
        }

        public ModelMetadata ModelMetadata { get; private set; }

        public string ModelStateKey { get; private set; }

        public bool ValidateAllProperties { get; set; }

        public bool SuppressValidation { get; set; }

        public void CombineWith(ModelValidationNode otherNode)
        {
            if (otherNode != null && !otherNode.SuppressValidation)
            {
                Validated += otherNode.Validated;
                Validating += otherNode.Validating;
                var otherChildNodes = otherNode._childNodes;
                for (var i = 0; i < otherChildNodes.Count; i++)
                {
                    var childNode = otherChildNodes[i];
                    _childNodes.Add(childNode);
                }
            }
        }

        private void OnValidated(ModelValidatedEventArgs e)
        {
            if (Validated != null)
            {
                Validated(this, e);
            }
        }

        private void OnValidating(ModelValidatingEventArgs e)
        {
            if (Validating != null)
            {
                Validating(this, e);
            }
        }

        private object TryConvertContainerToMetadataType(ModelValidationNode parentNode)
        {
            if (parentNode != null)
            {
                var containerInstance = parentNode.ModelMetadata.Model;
                if (containerInstance != null)
                {
                    var expectedContainerType = ModelMetadata.ContainerType;
                    if (expectedContainerType != null)
                    {
                        if (expectedContainerType.IsCompatibleWith(containerInstance))
                        {
                            return containerInstance;
                        }
                    }
                }
            }

            return null;
        }

        public void Validate(ModelValidationContext validationContext)
        {
            Validate(validationContext, parentNode: null);
        }

        public void Validate([NotNull] ModelValidationContext validationContext, ModelValidationNode parentNode)
        {
            if (SuppressValidation)
            {
                // no-op
                return;
            }

            // pre-validation steps
            var validatingEventArgs = new ModelValidatingEventArgs(validationContext, parentNode);
            OnValidating(validatingEventArgs);
            if (validatingEventArgs.Cancel)
            {
                return;
            }

            ValidateChildren(validationContext);
            ValidateThis(validationContext, parentNode);

            // post-validation steps
            var validatedEventArgs = new ModelValidatedEventArgs(validationContext, parentNode);
            OnValidated(validatedEventArgs);
        }

        private void ValidateChildren(ModelValidationContext validationContext)
        {
            for (var i = 0; i < _childNodes.Count; i++)
            {
                var child = _childNodes[i];
                var childValidationContext = new ModelValidationContext(validationContext, child.ModelMetadata);
                child.Validate(childValidationContext, this);
            }

            if (ValidateAllProperties)
            {
                ValidateProperties(validationContext);
            }
        }

        private void ValidateProperties(ModelValidationContext validationContext)
        {
            var modelState = validationContext.ModelState;

            var model = ModelMetadata.Model;
            var updatedMetadata = validationContext.MetadataProvider.GetMetadataForType(() => model, ModelMetadata.ModelType);

            foreach (var propertyMetadata in updatedMetadata.Properties)
            {
                // Only want to add errors to ModelState if something doesn't already exist for the property node,
                // else we could end up with duplicate or irrelevant error messages.
                var propertyKeyRoot = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, propertyMetadata.PropertyName);

                if (modelState.IsValidField(propertyKeyRoot))
                {
                    var propertyValidators = GetValidators(validationContext, propertyMetadata);
                    var propertyValidationContext = new ModelValidationContext(validationContext, propertyMetadata);
                    foreach (var propertyValidator in propertyValidators)
                    {
                        foreach (var propertyResult in propertyValidator.Validate(propertyValidationContext))
                        {
                            var thisErrorKey = ModelBindingHelper.CreatePropertyModelName(propertyKeyRoot, propertyResult.MemberName);
                            modelState.AddModelError(thisErrorKey, propertyResult.Message);
                        }
                    }
                }
            }
        }

        private void ValidateThis(ModelValidationContext validationContext, ModelValidationNode parentNode)
        {
            var modelState = validationContext.ModelState;
            if (!modelState.IsValidField(ModelStateKey))
            {
                return; // short-circuit
            }

            // If the Model at the current node is null and there is no parent, we cannot validate, and the 
            // DataAnnotationsModelValidator will throw. So we intercept here to provide a catch-all value-required 
            // validation error
            if (parentNode == null && ModelMetadata.Model == null)
            {
                var trueModelStateKey = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, ModelMetadata.GetDisplayName());
                modelState.AddModelError(trueModelStateKey, Resources.Validation_ValueNotFound);
                return;
            }

            var container = TryConvertContainerToMetadataType(parentNode);
            var validators = GetValidators(validationContext, ModelMetadata).ToArray();
            for (var i = 0; i < validators.Length; i++)
            {
                var validator = validators[i];
                foreach (var validationResult in validator.Validate(validationContext))
                {
                    var trueModelStateKey = ModelBindingHelper.CreatePropertyModelName(ModelStateKey, validationResult.MemberName);
                    modelState.AddModelError(trueModelStateKey, validationResult.Message);
                }
            }
        }

        private static IEnumerable<IModelValidator> GetValidators(ModelValidationContext validationContext, ModelMetadata metadata)
        {
            return validationContext.ValidatorProviders.SelectMany(vp => vp.GetValidators(metadata));
        }
    }
}
