// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class ActionRuntimeParameterInfo : IDictionary<string, object>
    {
        private IDictionary<string, object> _actionParameters = new Dictionary<string, object>();
        private IDictionary<string, ControllerPropertyAccessor> _controllerProperties = new Dictionary<string, ControllerPropertyAccessor>();
        private object _controllerObject;

        public ActionRuntimeParameterInfo(object controllerObject)
        {
            _controllerObject = controllerObject;
            var controllerPropertyAccessors = ControllerPropertyAccessor.GetPropertiesToActivate(_controllerObject.GetType());
            foreach (var accessor in controllerPropertyAccessors)
            {
                _controllerProperties[accessor.PropertyInfo.Name] = accessor;
            }
        }

        public object this[string key]
        {
            get
            {
                return _actionParameters[key];
            }

            set
            {
                _actionParameters[key] = value;
                if (_controllerProperties.TryGetValue(key, out var accessor))
                {
                    accessor.Set(_controllerObject, value);
                }
            }
        }

        public int Count
        {
            get
            {
                return _actionParameters.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _actionParameters.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return _actionParameters.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            _actionParameters.Add(key, value);
            if (_controllerProperties.TryGetValue(key, out var accessor))
            {
                accessor.Set(_controllerObject, value);
            }
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ContainsKey(item.Key);
        }

        public bool ContainsKey(string key)
        {
            return _actionParameters.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var item in _actionParameters)
            {
                yield return item;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        public bool Remove(string key)
        {
            if(_controllerProperties.TryGetValue(key, out var accessor))
            {
                accessor.Set(_controllerObject, null);
                return true;
            }

            if(_actionParameters.ContainsKey(key))
            {
                return _actionParameters.Remove(key);
            }

            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            if(_actionParameters.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actionParameters.GetEnumerator();
        }
    }
}
