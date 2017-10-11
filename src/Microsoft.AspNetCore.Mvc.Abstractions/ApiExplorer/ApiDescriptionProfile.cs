// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public abstract class ApiDescriptionProfile
    {
        public abstract string DisplayName { get; }

        public abstract bool IsMatch(ApiDescription description);

        public abstract void ApplyTo(ApiDescription description);
    }
}
