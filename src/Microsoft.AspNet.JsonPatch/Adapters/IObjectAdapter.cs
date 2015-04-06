// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.JsonPatch.Operations;

namespace Microsoft.AspNet.JsonPatch.Adapters
{
    public interface IObjectAdapter<T>
      where T : class
    {
        void Add(Operation<T> operation, T objectToApplyTo, Action<string> action);
        void Copy(Operation<T> operation, T objectToApplyTo, Action<string> action);
        void Move(Operation<T> operation, T objectToApplyTo, Action<string> action);
        void Remove(Operation<T> operation, T objectToApplyTo, Action<string> action);
        void Replace(Operation<T> operation, T objectToApplyTo, Action<string> action);
        void Test(Operation<T> operation, T objectToApplyTo, Action<string> action);
	}
}