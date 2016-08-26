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
        /// Creates a new <see cref="ViewComponentTagHelperDescriptorFactory"/>, 
        /// then creates <see cref="TagHelperDescriptor"/>s  for <see cref="ViewComponents"/> 
        /// in the given <see cref="IViewComponentDescriptorProvider"/>. 
        /// </summary>
        /// <param name="descriptorProvider">The provider of <see cref="ViewComponentDescriptor"/>s.</param>
        public ViewComponentTagHelperDescriptorFactory(IViewComponentDescriptorProvider descriptorProvider)
        {
            if (descriptorProvider == null)
            {
                throw new ArgumentNullException();
            }

            _descriptorProvider = descriptorProvider;
        }

        /// <summary>
        /// Creates <see cref="TagHelperDescriptor"/> representations of <see cref="ViewComponents"/>
        /// in an <see href="Assembly"/> represented by the given <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing 
        /// the <see cref="ViewComponents"/> s to translate.</param>
        /// <returns>A <see cref="IEnumerable{TagHelperDescriptor}"/>, 
        /// one for each <see cref="ViewComponents"/> .</returns>
        public IEnumerable<TagHelperDescriptor> CreateDescriptors(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(assemblyName);
            }

            var viewComponentDescriptors = _descriptorProvider
                .GetViewComponents()
                .Where(viewComponent => assemblyName.Equals(
                    viewComponent.TypeInfo.Assembly.GetName().Name, StringComparison.Ordinal));

            var tagHelperDescriptors = CreateDescriptors(viewComponentDescriptors);
            return tagHelperDescriptors;
        }

        /*
        private IEnumerable<TagHelperDescriptor> CreateDescriptors(
            IEnumerable<ViewComponentDescriptor> viewComponentDescriptors) =>
            viewComponentDescriptors.Select(viewComponentDescriptor => CreateDescriptor(viewComponentDescriptor));
            */

        private IEnumerable<TagHelperDescriptor> CreateDescriptors(IEnumerable<ViewComponentDescriptor> viewComponentDescriptors)
        {
            var tagHelperDescriptors = new List<TagHelperDescriptor>();

            foreach (var viewComponent in viewComponentDescriptors)
            {
                var descriptor = CreateDescriptor(viewComponent);
                tagHelperDescriptors.Add(descriptor);
            }

            return tagHelperDescriptors;
        }
        private TagHelperDescriptor CreateDescriptor(ViewComponentDescriptor viewComponentDescriptor)
        {
            var assemblyName = viewComponentDescriptor.TypeInfo.Assembly.GetName().Name;
            var tagName = GetTagName(viewComponentDescriptor);
            var typeName = GetTypeName(viewComponentDescriptor);

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = tagName,
                TypeName = typeName,
                AssemblyName = assemblyName,
                TagStructure = TagStructure.NormalOrSelfClosing,
            };

            SetAttributeDescriptors(viewComponentDescriptor, tagHelperDescriptor);
            SetRequiredAttributeDescriptors(viewComponentDescriptor, tagHelperDescriptor);

            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey, viewComponentDescriptor.ShortName);
            tagHelperDescriptor.PropertyBag.Add(
                ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperNameKey,
                GetTypeName(viewComponentDescriptor));

            return tagHelperDescriptor;
        }

        private void SetAttributeDescriptors(ViewComponentDescriptor viewComponentDescriptor,
            TagHelperDescriptor tagHelperDescriptor)
        {
            var methodParameters = viewComponentDescriptor.MethodInfo.GetParameters();
            var attributeDescriptors = new List<TagHelperAttributeDescriptor>();

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

                attributeDescriptors.Add(descriptor);
            }

            tagHelperDescriptor.Attributes = attributeDescriptors;
        }

        private void SetRequiredAttributeDescriptors(ViewComponentDescriptor viewComponentDescriptor,
            TagHelperDescriptor tagHelperDescriptor)
        {
            var methodParameters = viewComponentDescriptor.MethodInfo.GetParameters();
            var requiredAttributeDescriptors = new List<TagHelperRequiredAttributeDescriptor>();

            foreach (var parameter in methodParameters)
            {
                if (!parameter.HasDefaultValue)
                {
                    var requiredDescriptor = new TagHelperRequiredAttributeDescriptor
                    {
                        Name = TagHelperDescriptorFactory.ToHtmlCase(parameter.Name)
                    };

                    requiredAttributeDescriptors.Add(requiredDescriptor);
                }
            }

            tagHelperDescriptor.RequiredAttributes = requiredAttributeDescriptors;
        }

        private string GetTagName(ViewComponentDescriptor descriptor) =>
            $"vc:{TagHelperDescriptorFactory.ToHtmlCase(descriptor.ShortName)}";

        private string GetTypeName(ViewComponentDescriptor descriptor) =>
            $"__Generated__{descriptor.ShortName}ViewComponentTagHelper";
    }
}