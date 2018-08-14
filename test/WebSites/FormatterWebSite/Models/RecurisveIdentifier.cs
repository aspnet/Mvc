// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace FormatterWebSite
{
    // A System.Security.Principal.SecurityIdentifier like type that works on xplat
    public class RecurisveIdentifier
    {
        public RecurisveIdentifier(string identifier)
        {
            Value = identifier;
        }

        public string Value { get; }

        public RecurisveIdentifier AccountIdentifier => new RecurisveIdentifier(Value);
    }
}