﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class UnobtrusiveValidationAttributesGenerator
    {
        public static IDictionary<string, object> GetValidationAttributes(
            [NotNull] IEnumerable<ModelClientValidationRule> clientRules)
        {
            IDictionary<string, object> results = null;

            foreach (var rule in clientRules)
            {
                if (results == null)
                {
                    results = new Dictionary<string, object>(StringComparer.Ordinal);
                }

                var ruleName = "data-val-" + rule.ValidationType;

                ValidateUnobtrusiveValidationRule(rule, results, ruleName);

                results.Add(ruleName, rule.ErrorMessage ?? string.Empty);
                ruleName += "-";

                foreach (var kvp in rule.ValidationParameters)
                {
                    results.Add(ruleName + kvp.Key, kvp.Value ?? string.Empty);
                }
            }

            if (results != null)
            {
                results.Add("data-val", "true");
            }

            return results;
        }

        private static void ValidateUnobtrusiveValidationRule(ModelClientValidationRule rule,
            IDictionary<string, object> resultsDictionary, string dictionaryKey)
        {
            if (string.IsNullOrEmpty(rule.ValidationType))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeCannotBeEmpty(rule.GetType().FullName));
            }

            if (resultsDictionary.ContainsKey(dictionaryKey))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeUnique(rule.ValidationType));
            }

            if (rule.ValidationType.Any(c => !char.IsLower(c)))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeLegal(
                        rule.ValidationType,
                        rule.GetType().FullName));
            }

            foreach (var key in rule.ValidationParameters.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterCannotBeEmpty(rule.GetType().FullName));
                }

                if (!char.IsLower(key[0]) || key.Any(c => !char.IsLower(c) && !char.IsDigit(c)))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterMustBeLegal(
                            key,
                            rule.GetType().FullName));
                }
            }
        }
    }
}