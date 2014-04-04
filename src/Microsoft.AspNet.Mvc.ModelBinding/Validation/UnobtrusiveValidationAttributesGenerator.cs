using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public static class UnobtrusiveValidationAttributesGenerator
    {
        public static void GetValidationAttributes(IEnumerable<ModelClientValidationRule> clientRules, IDictionary<string, object> results)
        {
            if (clientRules == null)
            {
                throw new ArgumentNullException("clientRules");
            }
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            bool renderedRules = false;

            foreach (ModelClientValidationRule rule in clientRules)
            {
                renderedRules = true;
                string ruleName = "data-val-" + rule.ValidationType;

                ValidateUnobtrusiveValidationRule(rule, results, ruleName);

                results.Add(ruleName, rule.ErrorMessage ?? String.Empty);
                ruleName += "-";

                foreach (var kvp in rule.ValidationParameters)
                {
                    results.Add(ruleName + kvp.Key, kvp.Value ?? String.Empty);
                }
            }

            if (renderedRules)
            {
                results.Add("data-val", "true");
            }
        }

        private static void ValidateUnobtrusiveValidationRule(ModelClientValidationRule rule, IDictionary<string, object> resultsDictionary, string dictionaryKey)
        {
            if (String.IsNullOrWhiteSpace(rule.ValidationType))
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
                    Resources.FormatUnobtrusiveJavascript_ValidationTypeMustBeLegal(
                        rule.ValidationType,
                        rule.GetType().FullName));
            }

            foreach (var key in rule.ValidationParameters.Keys)
            {
                if (String.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException(
                        Resources.FormatUnobtrusiveJavascript_ValidationParameterCannotBeEmpty(rule.GetType().FullName));
                }

                if (!Char.IsLower(key.First()) || key.Any(c => !Char.IsLower(c) && !Char.IsDigit(c)))
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
