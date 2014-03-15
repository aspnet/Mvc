// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class TryGetValueProvider
    {
        private static readonly Dictionary<Type, TryGetValueDelegate> _tryGetValueDelegateCache =
            new Dictionary<Type, TryGetValueDelegate>();
        private static readonly ReaderWriterLockSlim _tryGetValueDelegateCacheLock = new ReaderWriterLockSlim();

        public static TryGetValueDelegate CreateInstance([NotNull] Type type)
        {
            TryGetValueDelegate result;

            // Cache delegates since properties of model types are re-evaluated numerous times
            _tryGetValueDelegateCacheLock.EnterReadLock();
            try
            {
                if (_tryGetValueDelegateCache.TryGetValue(type, out result))
                {
                    return result;
                }
            }
            finally
            {
                _tryGetValueDelegateCacheLock.ExitReadLock();
            }

            Type dictionaryType = type.ExtractGenericInterface(typeof(IDictionary<,>));

            // just wrap a call to the underlying IDictionary<TKey, TValue>.TryGetValue() where string can be cast to TKey
            if (dictionaryType != null)
            {
                var typeArguments = dictionaryType.GetGenericArguments();
                var keyType = typeArguments[0];
                var returnType = typeArguments[1];

                if (keyType.IsAssignableFrom(typeof(string)))
                {
                    var implementationMethod = typeof(TryGetValueProvider).GetRuntimeMethod("StrongTryGetValue",
                        new Type[] { keyType, returnType });
                    result = (TryGetValueDelegate)implementationMethod.CreateDelegate(typeof(TryGetValueDelegate));
                }
            }

            // wrap a call to the underlying IDictionary.Item()
            if (result == null && typeof(IDictionary).IsAssignableFrom(type))
            {
                result = WeakTryGetValue;
            }

            _tryGetValueDelegateCacheLock.EnterWriteLock();
            try
            {
                _tryGetValueDelegateCache[type] = result;
            }
            finally
            {
                _tryGetValueDelegateCacheLock.ExitWriteLock();
            }

            return result;
        }

        private static bool StrongTryGetValue<TKey, TValue>(object dictionary, string key, out object value)
        {
            var strongDict = (IDictionary<TKey, TValue>)dictionary;

            TValue strongValue;
            var success = strongDict.TryGetValue((TKey)(object)key, out strongValue);
            value = strongValue;
            return success;
        }

        private static bool WeakTryGetValue(object dictionary, string key, out object value)
        {
            var weakDict = (IDictionary)dictionary;

            var success = weakDict.Contains(key);
            value = success ? weakDict[key] : null;
            return success;
        }
    }
}
