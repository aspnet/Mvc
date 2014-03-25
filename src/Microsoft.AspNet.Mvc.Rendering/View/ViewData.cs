using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewData : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _data;
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
            _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _metadataProvider = metadataProvider;
        }

        /// <summary>
        /// <see cref="ViewData"/> copy constructor for use when model type does not change.
        /// </summary>
        public ViewData([NotNull] ViewData source)
            : this(source, source.Model)
        {
        }

        /// <summary>
        /// <see cref="ViewData"/> copy constructor for use when model type may change. This avoids exceptions a
        /// derived class may throw when <see cref="SetModel()"/> is called.
        /// </summary>
        public ViewData([NotNull] ViewData source, object model)
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

            SetModel(model);
        }

        public object Model
        {
            get { return _model; }
            set { SetModel(value); }
        }

        public ModelStateDictionary ModelState { get; private set; }

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

        // Remaining properties implement IDictionary<string, object> properties.

        // Do not just pass through to _data: Iterator should not throw a KeyNotFoundException.
        public object this[string index]
        {
            get
            {
                object result;
                _data.TryGetValue(index, out result);
                return result;
            }
            set
            {
                _data[index] = value;
            }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return _data.IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return _data.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _data.Values; }
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

        // Remaining methods implement IDictionary<string, object> methods.

        public void Add(string key, object value)
        {
            _data.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _data.Remove(item);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
