using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class UnobtrusiveValidationAttributesGenerator
    {
        public static void GetValidationAttributes([NotNull] IEnumerable<ModelClientValidationRule> clientRules,
            [NotNull] IDictionary<string, object> results)
        {
            var renderedRules = false;

            foreach (var rule in clientRules)
            {
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

            if (rule.ValidationType.Any(c => !char.IsLower(c)))
            {
                throw new InvalidOperationException(
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeLegal(
                        rule.ValidationType,
                        rule.GetType().FullName));
            }

            foreach (var key in rule.ValidationParameters.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterCannotBeEmpty(rule.GetType().FullName));
                }

                if (!char.IsLower(key.First()) || key.Any(c => !char.IsLower(c) && !char.IsDigit(c)))
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