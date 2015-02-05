﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
#if ASPNET50
using System.Runtime.Serialization;
#endif

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An exception which indicates multiple matches in action selection.
    /// </summary>
#if ASPNET50
    [Serializable]
#endif
    public class AmbiguousActionException : InvalidOperationException
    {
        public AmbiguousActionException(string message)
            : base(message)
        {
        }

#if ASPNET50
        protected AmbiguousActionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}