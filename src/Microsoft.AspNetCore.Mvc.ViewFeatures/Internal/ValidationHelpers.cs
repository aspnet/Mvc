// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public static class ValidationHelpers
    {
        private static readonly ModelStateEntry[] EmptyModelStateEntries = new ModelStateEntry[0];

        public static string GetModelErrorMessageOrDefault(ModelError modelError)
        {
            Debug.Assert(modelError != null);

            if (!string.IsNullOrEmpty(modelError.ErrorMessage))
            {
                return modelError.ErrorMessage;
            }

            // Default in the ValidationSummary case is no error message.
            return string.Empty;
        }

        public static string GetModelErrorMessageOrDefault(
            ModelError modelError,
            ModelStateEntry containingEntry,
            ModelExplorer modelExplorer)
        {
            Debug.Assert(modelError != null);
            Debug.Assert(containingEntry != null);
            Debug.Assert(modelExplorer != null);

            if (!string.IsNullOrEmpty(modelError.ErrorMessage))
            {
                return modelError.ErrorMessage;
            }

            // Default in the ValidationMessage case is a fallback error message.
            var attemptedValue = containingEntry.AttemptedValue ?? "null";
            return modelExplorer.Metadata.ModelBindingMessageProvider.ValueIsInvalidAccessor(attemptedValue);
        }

        // Returns non-null list of model states, which caller will render in order provided.
        public static IList<ModelStateEntry> GetModelStateList(
            ViewDataDictionary viewData,
            bool excludePropertyErrors)
        {
            if (excludePropertyErrors)
            {
                ModelStateEntry ms;
                viewData.ModelState.TryGetValue(viewData.TemplateInfo.HtmlFieldPrefix, out ms);

                if (ms != null)
                {
                    return new[] { ms };
                }
            }
            else if (viewData.ModelState.Count > 0)
            {
                var metadata = viewData.ModelMetadata;
                var modelStateDictionary = viewData.ModelState;
                var entries = new List<ModelStateEntry>();
                Visit(modelStateDictionary, modelStateDictionary.Root, metadata, entries);

                if (entries.Count < modelStateDictionary.Count)
                {
                    // Account for entries in the ModelStateDictionary that do not have corresponding ModelMetadata values.
                    foreach (var entry in modelStateDictionary)
                    {
                        if (!entries.Contains(entry.Value))
                        {
                            entries.Add(entry.Value);
                        }
                    }
                }

                return entries;
            }

            return EmptyModelStateEntries;
        }

        private static void Visit(
            ModelStateDictionary dictionary,
            ModelStateEntry modelStateEntry,
            ModelMetadata metadata,
            List<ModelStateEntry> orderedModelStateEntries)
        {
            if (metadata.ElementMetadata != null && modelStateEntry.Children != null)
            {
                foreach (var indexEntry in modelStateEntry.Children)
                {
                    Visit(dictionary, indexEntry, metadata.ElementMetadata, orderedModelStateEntries);
                }
            }

            for (var i = 0; i < metadata.Properties.Count; i++)
            {
                var propertyMetadata = metadata.Properties[i];
                var propertyModelStateEntry = modelStateEntry.GetModelStateForProperty(propertyMetadata.PropertyName);
                if (propertyModelStateEntry != null)
                {
                    Visit(dictionary, propertyModelStateEntry, propertyMetadata, orderedModelStateEntries);
                }
            }

            if (!modelStateEntry.IsContainerNode)
            {
                orderedModelStateEntries.Add(modelStateEntry);
            }
        }
    }
}
