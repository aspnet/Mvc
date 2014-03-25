﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewData : DynamicObject
    {
        private readonly Dictionary<object, dynamic> _data;
        private object _model;
        private ModelMetadata _modelMetadata;
        private IModelMetadataProvider _metadataProvider;

        public ViewData([NotNull] IModelMetadataProvider metadataProvider)
            : this(metadataProvider, new ModelStateDictionary())
        {
        }

        public ViewData([NotNull] IModelMetadataProvider metadataProvider, [NotNull] ModelStateDictionary modelState)
        {
            ModelState = modelState;
            _data = new Dictionary<object, dynamic>();
            _metadataProvider = metadataProvider;
        }

        public ViewData([NotNull] ViewData source)
            : this(source.MetadataProvider)
        {
            _modelMetadata = source.ModelMetadata;
            
            foreach (var entry in source.ModelState)
            {
                ModelState.Add(entry.Key, entry.Value);
            }
            
            foreach (var entry in source)
            {
                _data.Add(entry.Key, entry.Value);
            }

            SetModel(source.Model);
        }

        public object Model
        {
            get { return _model; }
            set { SetModel(value); }
        }

        public ModelStateDictionary ModelState { get; private set; }

        public dynamic this[string index]
        {
            get
            {
                dynamic result;
                TryGetValue(index, out result);

                return result;
            }
            set
            {
                _data[index] = value;
            }
        }

        public virtual ModelMetadata ModelMetadata
        {
            get
            {
                return _modelMetadata;
            }
            set
            {
                _modelMetadata = value;
            }
        }

        /// <summary>
        /// Provider for subclasses that need it to override <see cref="ModelMetadata"/>.
        /// </summary>
        protected IModelMetadataProvider MetadataProvider
        {
            get { return _metadataProvider; }
        }
        
        public Dictionary<object, dynamic>.Enumerator GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            // Indexer's result will be null when TryGetValue fails. Never return false; that confuses the runtime.
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
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
                // Unable to determine model metadata.
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
