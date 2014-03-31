using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class ExpressionMetadataProvider
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        internal static ModelMetadata FromLambdaExpression<TParameter, TValue>(Expression<Func<TParameter, TValue>> expression,
                                                                               ViewDataDictionary<TParameter> viewData,
                                                                               ModelMetadataProvider metadataProvider)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (viewData == null)
            {
                throw new ArgumentNullException("viewData");
            }

            string propertyName = null;
            Type containerType = null;
            bool legalExpression = false;

            // Need to verify the expression is valid; it needs to at least end in something
            // that we can convert to a meaningful string for model binding purposes

            switch (expression.Body.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    // ArrayIndex always means a single-dimensional indexer; multi-dimensional indexer is a method call to Get()
                    legalExpression = true;
                    break;

                case ExpressionType.Call:
                    // Only legal method call is a single argument indexer/DefaultMember call
                    legalExpression = ExpressionHelper.IsSingleArgumentIndexer(expression.Body);
                    break;

                case ExpressionType.MemberAccess:
                    // Property/field access is always legal
                    MemberExpression memberExpression = (MemberExpression)expression.Body;
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
                throw new InvalidOperationException(MvcResources.TemplateHelpers_TemplateLimitations);
            }

            TParameter container = viewData.Model;
            Func<object> modelAccessor = () =>
            {
                try
                {
                    return CachedExpressionCompiler.Process(expression)(container);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            };

            return GetMetadataFromProvider(modelAccessor, typeof(TValue), propertyName, container, containerType, metadataProvider);
        }

        internal static ModelMetadata FromStringExpression(string expression, ViewDataDictionary viewData, ModelMetadataProvider metadataProvider)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (viewData == null)
            {
                throw new ArgumentNullException("viewData");
            }
            if (expression.Length == 0)
            {
                // Empty string really means "model metadata for the current model"
                return FromModel(viewData, metadataProvider);
            }

            ViewDataInfo vdi = viewData.GetViewDataInfo(expression);
            object container = null;
            Type containerType = null;
            Type modelType = null;
            Func<object> modelAccessor = null;
            string propertyName = null;

            if (vdi != null)
            {
                if (vdi.Container != null)
                {
                    container = vdi.Container;
                    containerType = vdi.Container.GetType();
                }

                modelAccessor = () => vdi.Value;

                if (vdi.PropertyDescriptor != null)
                {
                    propertyName = vdi.PropertyDescriptor.Name;
                    modelType = vdi.PropertyDescriptor.PropertyType;
                }
                else if (vdi.Value != null)
                {
                    // We only need to delay accessing properties (for LINQ to SQL)
                    modelType = vdi.Value.GetType();
                }
            }
            else if (viewData.ModelMetadata != null)
            {
                //  Try getting a property from ModelMetadata if we couldn't find an answer in ViewData
                ModelMetadata propertyMetadata = viewData.ModelMetadata.Properties.Where(p => p.PropertyName == expression).FirstOrDefault();
                if (propertyMetadata != null)
                {
                    return propertyMetadata;
                }
            }

            return GetMetadataFromProvider(modelAccessor, modelType ?? typeof(string), propertyName, container, containerType, metadataProvider);
        }

        private static ModelMetadata FromModel(ViewDataDictionary viewData, ModelMetadataProvider metadataProvider)
        {
            return viewData.ModelMetadata ?? GetMetadataFromProvider(null, typeof(string), null, null, null, metadataProvider);
        }

        private static ModelMetadata GetMetadataFromProvider(Func<object> modelAccessor, Type modelType, string propertyName, object container, Type containerType, ModelMetadataProvider metadataProvider)
        {
            metadataProvider = metadataProvider ?? ModelMetadataProviders.Current;
            if (containerType != null && !String.IsNullOrEmpty(propertyName))
            {
                ModelMetadata metadata = metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
                if (metadata != null)
                {
                    metadata.Container = container;
                }
                return metadata;
            }
            return metadataProvider.GetMetadataForType(modelAccessor, modelType);
        }
    }
}