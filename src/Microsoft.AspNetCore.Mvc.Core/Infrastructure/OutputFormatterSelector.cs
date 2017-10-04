// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public abstract class OutputFormatterSelector
    {
        public abstract IOutputFormatter SelectFormatter(OutputFormatterWriteContext context, IList<IOutputFormatter> formatters, MediaTypeCollection mediaTypes);
    }
}
