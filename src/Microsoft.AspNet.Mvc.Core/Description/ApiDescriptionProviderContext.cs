// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Description
{
    public class ApiDescriptionProviderContext
    {
	    public ApiDescriptionProviderContext([NotNull] IReadOnlyList<ActionDescriptor> actions)
	    {
            Actions = actions;

            Results = new List<ApiDescription>();
	    }

        public IReadOnlyList<ActionDescriptor> Actions { get; private set; }

        public List<ApiDescription> Results { get; private set; }
    }
}