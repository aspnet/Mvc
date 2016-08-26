// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Host;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers
{
    /// <summary>
    /// Provides methods to create tag helper representations of view components.
    /// </summary>
    public class ViewComponentTagHelperDescriptorFactory
    {
        private readonly IViewComponentDescriptorProvider _descriptorProvider;

        /// <summary>
        /// Creates a new ViewComponentTagHelperDescriptorFactory than creates tag helper descriptors for
        /// view components in the given descriptorProvider.
        /// </summary>
        /// <param name="descriptorProvider">The provider of view component descriptors.</param>
        public ViewComponentTagHelperDescriptorFactory(IViewComponentDescriptorProvider descriptorProvider)
        {
            _descriptorProvider = descriptorProvider;
        }

        /// <summary>
        /// Creates <see cref="TagHelperDescriptor"/> representations of view components in a given assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the view components to translate.</param>
        /// <returns>A <see cref="IEnumerable{TagHelperDescriptor}"/>, one for each view component.</returns>
        public IEnumerable<TagHelperDescriptor> CreateDescriptors(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(assemblyName);
            }

            var viewComponentDescriptors = _descriptorProvider.GetViewComponents()
                .Where(viewComponent => assemblyName.Equals(
                viewComponent.TypeInfo.Assembly.GetName().Name));

            return CreateDescriptors(viewComponentDescriptors);
        }

        private IEnumerable<TagHelperDescriptor> CreateDescriptors(
            IEnumerable<ViewComponentDescriptor> viewComponentDescriptors)
        {
            var tagHelperDescriptors = new List<TagHelperDescriptor>();

            foreach (var viewComponentDescriptor in viewComponentDescriptors)
            {
                var tagHelperDescriptor = CreateDescriptor(viewComponentDescriptor);
                tagHelperDescriptors.Add(tagHelperDescriptor);
            }

            return tagHelperDescriptors;
        }

        private TagHelperDescriptor CreateDescriptor(ViewComponentDescriptor viewComponentDescriptor)
        {
            // Fill in the attribute and required attribute descriptors.
            IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors;
            IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors;
            if (!TryGetAttributeDescriptors(viewComponentDescriptor,
                out attributeDescriptors,
                out requiredAttributeDescriptors))
            {
                // After adding view component name validation,
                // this exception will make sense.
                throw new Exception("Unable to resolve view component descriptor to tag helper descriptor.");
            }

            var assemblyName = viewComponentDescriptor.TypeInfo.Assembly.GetName().Name;
            var tagName = ViewComponentTagHelperDescriptorConventions.GetTagName(viewComponentDescriptor);
            var typeName = ViewComponentTagHelperDescriptorConventions.GetTypeName(viewComponentDescriptor);

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = tagName,
                TypeName = typeName,
                AssemblyName = assemblyName,
                Attributes = attributeDescriptors,
                RequiredAttributes = requiredAttributeDescriptors,
                TagStructure = TagStructure.NormalOrSelfClosing,
            };

            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey, viewComponentDescriptor.ShortName);
            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperNameKey,
                ViewComponentTagHelperDescriptorConventions.GetTypeName(viewComponentDescriptor));

            return tagHelperDescriptor;
        }

        // TODO: Add support for customization of HtmlTargetElement, HtmlAttributeName.
        private bool TryGetAttributeDescriptors(
            ViewComponentDescriptor viewComponentDescriptor,
            out IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors,
            out IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors
            )
        {
            var methodParameters = viewComponentDescriptor.MethodInfo.GetParameters();
            var descriptors = new List<TagHelperAttributeDescriptor>();
            var requiredDescriptors = new List<TagHelperRequiredAttributeDescriptor>();

            foreach (var parameter in methodParameters)
            {
                var lowerKebabName = TagHelperDescriptorFactory.ToHtmlCase(parameter.Name);
                var descriptor = new TagHelperAttributeDescriptor
                {
                    Name = lowerKebabName,
                    PropertyName = parameter.Name,
                    TypeName = parameter.ParameterType.FullName
                };

                var tagHelperType = Type.GetType(descriptor.TypeName);
                if (tagHelperType.Equals(typeof(string)))
                {
                    descriptor.IsStringProperty = true;
                }

                descriptors.Add(descriptor);

                if (!parameter.HasDefaultValue)
                {
                    var requiredDescriptor = new TagHelperRequiredAttributeDescriptor
                    {
                        Name = lowerKebabName
                    };

                    requiredDescriptors.Add(requiredDescriptor);
                }
            }

            attributeDescriptors = descriptors;
            requiredAttributeDescriptors = requiredDescriptors;

            return true;
        }
    }
}