// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewData : DynamicObject
    {
        private readonly Dictionary<string, dynamic> _data;
        private readonly ModelStateDictionary _modelState;
        private object _model;
        private TemplateInfo _templateInfo;
        private ModelMetadata _modelMetadata;
        private IModelMetadataProvider _metadataProvider;

        public ViewData([NotNull] IModelMetadataProvider metadataProvider)
            : this()
        {
            _metadataProvider = metadataProvider;
        }

        public ViewData([NotNull] ViewData source)
            : this()
        {
            foreach (var entry in source._data)
            {
                // K10 compiler can't handle _data.Add() due to a missing requirement for dynamic compilation.
                _data[entry.Key] = entry.Value;
            }

            foreach (var entry in source.ModelState)
            {
                _modelState.Add(entry.Key, entry.Value);
            }

            _metadataProvider = source._metadataProvider;
            _templateInfo = source.TemplateInfo;
            SetModel(source.Model);
        }

        private ViewData()
        {
            _data = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            _modelState = new ModelStateDictionary();
            _templateInfo = new TemplateInfo();
        }

        public object Model
        {
            get { return _model; }
            set { SetModel(value); }
        }

        public ModelStateDictionary ModelState
        {
            get { return _modelState; }
        }

        public dynamic this[string index]
        {
            get
            {
                dynamic result;
                _data.TryGetValue(index, out result);
                return result;
            }
            set
            {
                _data[index] = (dynamic)value;
            }
        }

        public TemplateInfo TemplateInfo
        {
            get { return _templateInfo; }
            set { _templateInfo = value; }
        }

        public ModelMetadata ModelMetadata
        {
            get
            {
                return _modelMetadata ?? DefaultModelMetadata;
            }
            set
            {
                _modelMetadata = value;
            }
        }

        /// <summary>
        /// Fallback <see cref="ModelMetadata"/> if this class is unable to determine a value. Subclasses may for
        /// example have additional context allowing them to determine appropriate metadata when the
        /// <see cref="Model"/> is <c>null</c>.
        /// </summary>
        protected virtual ModelMetadata DefaultModelMetadata
        {
            get { return null; }
        }

        /// <summary>
        /// Provided for subclasses needing this provider to evaluate a <see cref="DefaultModelMetadata"/> value.
        /// </summary>
        protected IModelMetadataProvider MetadataProvider
        {
            get { return _metadataProvider; }
        }

        #region DynamicObject
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            dynamic value;
            var gotMember = TryGetValue(binder.Name, out value);
            result = (object)value;

            // result will be null if TryGetValue fails, matching behaviour of indexer. But return failure indication
            // to let caller know member does not exist, avoiding confusion with a null member.
            return gotMember;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // This cast should always succeed.
            var dynamicValue = (dynamic)value;
            _data[binder.Name] = dynamicValue;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException("Invalid number of indexes");
            }

            var index = indexes[0];

            // This cast should always succeed.
            result = this[(string)index];
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException("Invalid number of indexes");
            }

            var index = indexes[0];

            // This cast should always succeed.
            this[(string)index] = value;
            return true;
        }
        #endregion

        public bool TryGetValue(string key, out dynamic value)
        {
            return _data.TryGetValue(key, out value);
        }

        // This method will execute before the derived type's instance constructor executes. Derived types must
        // be aware of this and should plan accordingly. For example, the logic in SetModel() should be simple
        // enough so as not to depend on the "this" pointer referencing a fully constructed object.
        protected virtual void SetModel(object value)
        {
            _model = value;
            if (value == null)
            {
                // Unable to determine model metadata. Use default, if any.
                _modelMetadata = null;
            }
            else if (_modelMetadata == null || value.GetType() != ModelMetadata.ModelType)
            {
                // Reset or override model metadata based on new value type.
                _modelMetadata = _metadataProvider.GetMetadataForType(() => value, value.GetType());
            }
        }
    }
}
