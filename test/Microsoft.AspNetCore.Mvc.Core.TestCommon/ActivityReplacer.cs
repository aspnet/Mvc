// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Mvc
{
    public class ActivityReplacer : IDisposable
    {
        private readonly Activity _oldActivity;
        private readonly Activity _activity;

        public ActivityReplacer()
        {
            _oldActivity = Activity.Current;
            _activity = new Activity("Test");
            _activity.Start();
            Activity.Current = _activity;
        }

        public void Dispose()
        {
            Debug.Assert(Activity.Current == _activity);
            _activity.Stop();
            Activity.Current = _oldActivity;
        }
    }
}
