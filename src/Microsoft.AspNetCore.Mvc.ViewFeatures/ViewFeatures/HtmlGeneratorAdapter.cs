// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Adapts an <see cref="IHtmlGenerator"/> implementation to provide all of the <see cref="IHtmlGeneratorTutu"/>
    /// interface.
    /// </summary>
    public class HtmlGeneratorAdapter : IHtmlGeneratorTutu
    {
        private readonly IHtmlGenerator _innerGenerator;

        private HtmlGeneratorAdapter(IHtmlGenerator innerGenerator)
        {
            _innerGenerator = innerGenerator;
        }

        /// <inheritdoc />
        public string IdAttributeDotReplacement => _innerGenerator.IdAttributeDotReplacement;

        /// <summary>
        /// Return a full <see cref="IHtmlGeneratorTutu"/> implementation based on the given
        /// <paramref name="generator"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/> implementation to cast or wrap.</param>
        /// <returns>The <see cref="IHtmlGeneratorTutu"/> implementation.</returns>
        public static IHtmlGeneratorTutu GetTuTu(IHtmlGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            var generatorTuTu = generator as IHtmlGeneratorTutu;
            if (generatorTuTu != null)
            {
                return generatorTuTu;
            }

            return new HtmlGeneratorAdapter(generator);
        }

        /// <inheritdoc />
        public string Encode(object value)
        {
            return _innerGenerator.Encode(value);
        }

        /// <inheritdoc />
        public string Encode(string value)
        {
            return _innerGenerator.Encode(value);
        }

        /// <inheritdoc />
        public string FormatValue(object value, string format)
        {
            return _innerGenerator.FormatValue(value, format);
        }

        /// <inheritdoc />
        public TagBuilder GenerateActionLink(
            ViewContext viewContext,
            string linkText,
            string actionName,
            string controllerName,
            string protocol,
            string hostname,
            string fragment,
            object routeValues,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateActionLink(
                viewContext,
                linkText,
                actionName,
                controllerName,
                protocol,
                hostname,
                fragment,
                routeValues,
                htmlAttributes);
        }

        /// <inheritdoc />
        public IHtmlContent GenerateAntiforgery(ViewContext viewContext)
        {
            return _innerGenerator.GenerateAntiforgery(viewContext);
        }

        /// <inheritdoc />
        public TagBuilder GenerateCheckBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            bool? isChecked,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateCheckBox(viewContext, modelExplorer, expression, isChecked, htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateCheckBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            bool? isChecked,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateCheckBox(
                viewContext,
                modelExplorer,
                expression.ToString(),
                isChecked,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateForm(
            ViewContext viewContext,
            string actionName,
            string controllerName,
            object routeValues,
            string method,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateForm(
                viewContext,
                actionName,
                controllerName,
                routeValues,
                method,
                htmlAttributes);
        }

        /// <inheritdoc />
        public IHtmlContent GenerateGroupsAndOptions(string optionLabel, IEnumerable<SelectListItem> selectList)
        {
            return _innerGenerator.GenerateGroupsAndOptions(optionLabel, selectList);
        }

        /// <inheritdoc />
        public TagBuilder GenerateHidden(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            bool useViewData,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateHidden(
                viewContext,
                modelExplorer,
                expression,
                value,
                useViewData,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateHidden(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            bool useViewData,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateHidden(
                viewContext,
                modelExplorer,
                expression.ToString(),
                value,
                useViewData,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateHiddenForCheckbox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression)
        {
            return _innerGenerator.GenerateHiddenForCheckbox(viewContext, modelExplorer, expression);
        }

        /// <inheritdoc />
        public TagBuilder GenerateHiddenForCheckbox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression)
        {
            return _innerGenerator.GenerateHiddenForCheckbox(viewContext, modelExplorer, expression.ToString());
        }

        /// <inheritdoc />
        public TagBuilder GenerateLabel(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            string labelText,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateLabel(viewContext, modelExplorer, expression, labelText, htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateLabel(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            string labelText,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateLabel(
                viewContext,
                modelExplorer,
                expression.ToString(),
                labelText,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GeneratePassword(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            object htmlAttributes)
        {
            return _innerGenerator.GeneratePassword(viewContext, modelExplorer, expression, value, htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GeneratePassword(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            object htmlAttributes)
        {
            return _innerGenerator.GeneratePassword(
                viewContext,
                modelExplorer,
                expression.ToString(),
                value,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateRadioButton(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            bool? isChecked,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateRadioButton(
                viewContext,
                modelExplorer,
                expression,
                value,
                isChecked,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateRadioButton(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            bool? isChecked,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateRadioButton(
                viewContext,
                modelExplorer,
                expression.ToString(),
                value,
                isChecked,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateRouteForm(
            ViewContext viewContext,
            string routeName,
            object routeValues,
            string method,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateRouteForm(viewContext, routeName, routeValues, method, htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateRouteLink(
            ViewContext viewContext,
            string linkText,
            string routeName,
            string protocol,
            string hostName,
            string fragment,
            object routeValues,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateRouteLink(
                viewContext,
                linkText,
                routeName,
                protocol,
                hostName,
                fragment,
                routeValues,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateSelect(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            string expression,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateSelect(
                viewContext,
                modelExplorer,
                optionLabel,
                expression,
                selectList,
                allowMultiple,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateSelect(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            StringValuesTutu expression,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateSelect(
                viewContext,
                modelExplorer,
                optionLabel,
                expression.ToString(),
                selectList,
                allowMultiple,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateSelect(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            string expression,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateSelect(
                viewContext,
                modelExplorer,
                optionLabel,
                expression,
                selectList,
                currentValues,
                allowMultiple,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateSelect(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            StringValuesTutu expression,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateSelect(
                viewContext,
                modelExplorer,
                optionLabel,
                expression.ToString(),
                selectList,
                currentValues,
                allowMultiple,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateTextArea(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            int rows,
            int columns,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateTextArea(
                viewContext,
                modelExplorer,
                expression,
                rows,
                columns,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateTextArea(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            int rows,
            int columns,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateTextArea(
                viewContext,
                modelExplorer,
                expression.ToString(),
                rows,
                columns,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateTextBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            string format,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateTextBox(
                viewContext,
                modelExplorer,
                expression,
                value,
                format,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateTextBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            string format,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateTextBox(
                viewContext,
                modelExplorer,
                expression.ToString(),
                value,
                format,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateValidationMessage(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            string message,
            string tag,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateValidationMessage(
                viewContext,
                modelExplorer,
                expression,
                message,
                tag,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateValidationMessage(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            string message,
            string tag,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateValidationMessage(
                viewContext,
                modelExplorer,
                expression.ToString(),
                message,
                tag,
                htmlAttributes);
        }

        /// <inheritdoc />
        public TagBuilder GenerateValidationSummary(
            ViewContext viewContext,
            bool excludePropertyErrors,
            string message,
            string headerTag,
            object htmlAttributes)
        {
            return _innerGenerator.GenerateValidationSummary(
                viewContext,
                excludePropertyErrors,
                message,
                headerTag,
                htmlAttributes);
        }

        /// <inheritdoc />
        public ICollection<string> GetCurrentValues(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            bool allowMultiple)
        {
            return _innerGenerator.GetCurrentValues(viewContext, modelExplorer, expression, allowMultiple);
        }

        /// <inheritdoc />
        public ICollection<string> GetCurrentValues(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            bool allowMultiple)
        {
            return _innerGenerator.GetCurrentValues(viewContext, modelExplorer, expression.ToString(), allowMultiple);
        }
    }
}
