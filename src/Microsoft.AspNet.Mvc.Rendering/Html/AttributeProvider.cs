// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering.Html
{
    public static class AttributeProvider
    {
        public static IDictionary<string, object> GetValidationAttributes(
            [NotNull] IEnumerable<ModelClientValidationRule> clientRules)
        {
            IDictionary<string, object> results = null;
            var renderedRules = false;
            foreach (ModelClientValidationRule rule in clientRules)
            {
                if (results == null)
                {
                    results = new Dictionary<string, object>();
                }

                renderedRules = true;
                var ruleName = "data-val-" + rule.ValidationType;

                ValidateUnobtrusiveValidationRule(rule, results, ruleName);

                results.Add(ruleName, rule.ErrorMessage ?? string.Empty);
                ruleName += "-";

                foreach (var kvp in rule.ValidationParameters)
                {
                    results.Add(ruleName + kvp.Key, kvp.Value ?? string.Empty);
                }
            }

            if (renderedRules)
            {
                results.Add("data-val", "true");
            }

            return results;
        }

        private static void ValidateUnobtrusiveValidationRule(ModelClientValidationRule rule,
            IDictionary<string, object> resultsDictionary, string dictionaryKey)
        {
            if (string.IsNullOrWhiteSpace(rule.ValidationType))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeCannotBeEmpty(rule.GetType().FullName));
            }

            if (resultsDictionary.ContainsKey(dictionaryKey))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeUnique(rule.ValidationType));
            }

            if (rule.ValidationType.Any(c => !Char.IsLower(c)))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeLegal(rule.ValidationType,
                        rule.GetType().FullName));
            }

            foreach (var key in rule.ValidationParameters.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterCannotBeEmpty(
                            rule.GetType().FullName));
                }

                if (!Char.IsLower(key.First()) || key.Any(c => !Char.IsLower(c) && !Char.IsDigit(c)))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterMustBeLegal(key,
                            rule.GetType().FullName));
                }
            }
        }
    }
}
