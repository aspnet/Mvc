// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.DataProtection;

namespace Microsoft.AspNet.Mvc
{
    // TODO: Stub :Replace with actual implementation
    internal sealed class AntiForgeryTokenSerializer : IAntiForgeryTokenSerializer
    {
        private readonly IDataProtector _cryptoSystem;

        internal AntiForgeryTokenSerializer(IDataProtector cryptoSystem)
        {
            _cryptoSystem = cryptoSystem;
        }

        public AntiForgeryToken Deserialize(string serializedToken)
        {
            throw new NotImplementedException();
        }

        public string Serialize(AntiForgeryToken token)
        {
            throw new NotImplementedException();
        }
    }
}