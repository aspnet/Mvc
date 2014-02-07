// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    // Returns a user-specified binder for a given type.
    public sealed class SimpleModelBinderProvider : IModelBinderProvider
    {
        private readonly Func<IModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public SimpleModelBinderProvider(Type modelType, IModelBinder modelBinder)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinder == null)
            {
                throw Error.ArgumentNull("modelBinder");
            }

            _modelType = modelType;
            _modelBinderFactory = () => modelBinder;
        }

        public SimpleModelBinderProvider(Type modelType, Func<IModelBinder> modelBinderFactory)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }
            if (modelBinderFactory == null)
            {
                throw Error.ArgumentNull("modelBinderFactory");
            }

            _modelType = modelType;
            _modelBinderFactory = modelBinderFactory;
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public bool SuppressPrefixCheck { get; set; }

        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            if (modelType == ModelType)
            {
                if (SuppressPrefixCheck)
                {
                    // If we're suppressing a prefix check, then we don't need any further info from the ActionContext
                    // to know that we're using this binder. 
                    return _modelBinderFactory();
                }
                else
                {
                    return new SimpleModelBinder(this);
                }
            }
            return null;
        }

        // Helper binder to do the prefix check before invoking into the user's binder. 
        private class SimpleModelBinder : IModelBinder
        {
            private readonly SimpleModelBinderProvider _parent;

            public SimpleModelBinder(SimpleModelBinderProvider parent)
            {
                _parent = parent;
            }

            public bool BindModel(ModelBindingContext bindingContext)
            {
                Contract.Assert(!_parent.SuppressPrefixCheck); // wouldn't have even created this binder 
                if (bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                {
                    IModelBinder binder = _parent._modelBinderFactory();
                    return binder.BindModel(bindingContext);
                }
                return false;
            }
        }
    }
}
