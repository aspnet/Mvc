// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class ExpressionMetadataProvider
    {
        public static ModelExplorer FromLambdaExpression<TModel, TValue>(
            [NotNull] Expression<Func<TModel, TValue>> expression,
            [NotNull] ViewDataDictionary<TModel> viewData,
            IModelMetadataProvider metadataProvider)
        {
            string propertyName = null;
            Type containerType = null;
            var legalExpression = false;

            // Need to verify the expression is valid; it needs to at least end in something
            // that we can convert to a meaningful string for model binding purposes

            switch (expression.Body.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    // ArrayIndex always means a single-dimensional indexer;
                    // multi-dimensional indexer is a method call to Get().
                    legalExpression = true;
                    break;

                case ExpressionType.Call:
                    // Only legal method call is a single argument indexer/DefaultMember call
                    legalExpression = ExpressionHelper.IsSingleArgumentIndexer(expression.Body);
                    break;

                case ExpressionType.MemberAccess:
                    // Property/field access is always legal
                    var memberExpression = (MemberExpression)expression.Body;
                    propertyName = memberExpression.Member is PropertyInfo ? memberExpression.Member.Name : null;
                    containerType = memberExpression.Expression.Type;
                    legalExpression = true;
                    break;

                case ExpressionType.Parameter:
                    // Parameter expression means "model => model", so we delegate to FromModel
                    return FromModel(viewData, metadataProvider);
            }

            if (!legalExpression)
            {
                throw new InvalidOperationException(Resources.TemplateHelpers_TemplateLimitations);
            }

            var container = viewData.Model;
            Func<object, object> modelAccessor = (c) =>
            {
                try
                {
                    return CachedExpressionCompiler.Process(expression)((TModel)c);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            };

            ModelMetadata metadata;
            if (propertyName == null)
            {
                // Ex: 
                //    m => 5 (arbitrary expression)
                //    m => foo (arbitrary expression)
                //    m => m.Widgets[0] (expression ending with non-property-access)
                metadata = metadataProvider.GetMetadataForType(typeof(TValue));
            }
            else
            {
                // Ex: 
                //    m => m.Color (simple property access)
                //    m => m.Color.Red (nested property access)
                //    m => m.Widgets[0].Size (expression ending with property-access)
                metadata = metadataProvider.GetMetadataForType(containerType).Properties[propertyName];
            }

            return viewData.ModelExplorer.GetExplorerForExpression(metadata, modelAccessor);
        }

        public static ModelExplorer FromStringExpression(
            string expression,
            [NotNull] ViewDataDictionary viewData,
            IModelMetadataProvider metadataProvider)
        {
            if (string.IsNullOrEmpty(expression))
            {
                // Empty string really means "ModelMetadata for the current model".
                return FromModel(viewData, metadataProvider);
            }

            var viewDataInfo = ViewDataEvaluator.Eval(viewData, expression);

            if (viewDataInfo == null)
            {
                //  Try getting a property from ModelMetadata if we couldn't find an answer in ViewData
                var propertyExplorer = viewData.ModelExplorer.GetExplorerForProperty(expression);
                if (propertyExplorer != null)
                {
                    return propertyExplorer;
                }
            }

            if (viewDataInfo != null)
            {
                Func<object, object> modelAccessor = (ignore) => viewDataInfo.Value;

                ModelExplorer containerExplorer = viewData.ModelExplorer;
                if (viewDataInfo.Container != null)
                {
                    containerExplorer = metadataProvider.GetModelExplorerForType(
                        viewDataInfo.Container.GetType(), 
                        viewDataInfo.Container);
                }

                if (viewDataInfo.PropertyInfo != null)
                {
                    // We've identified a property access, which provides us with accurate metadata.
                    var containerType = viewDataInfo.Container?.GetType() ?? viewDataInfo.PropertyInfo.DeclaringType;
                    var containerMetadata = metadataProvider.GetMetadataForType(viewDataInfo.Container.GetType());
                    var propertyMetadata = containerMetadata.Properties[viewDataInfo.PropertyInfo.Name];

                    return containerExplorer.GetExplorerForExpression(propertyMetadata, modelAccessor);
                }
                else if (viewDataInfo.Value != null)
                {
                    // We have a value, even though we may not know where it came from.
                    var valueMetadata = metadataProvider.GetMetadataForType(viewDataInfo.Value.GetType());
                    return containerExplorer.GetExplorerForExpression(valueMetadata, modelAccessor);
                }
            }

            // Treat the expression as string if we don't find anything better.
            var stringMetadata = metadataProvider.GetMetadataForType(typeof(string));
            return viewData.ModelExplorer.GetExplorerForExpression(stringMetadata, modelAccessor: null);
        }

        private static ModelExplorer FromModel(
            [NotNull] ViewDataDictionary viewData,
            IModelMetadataProvider metadataProvider)
        {
            if (viewData.ModelMetadata.ModelType == typeof(object))
            {
                // Use common simple type rather than object so e.g. Editor() at least generates a TextBox.
                var model = viewData.Model == null ? null : Convert.ToString(viewData.Model, CultureInfo.CurrentCulture);
                return metadataProvider.GetModelExplorerForType(typeof(string), null);
            }
            else
            {
                return viewData.ModelExplorer;
            }
        }
    }
}
