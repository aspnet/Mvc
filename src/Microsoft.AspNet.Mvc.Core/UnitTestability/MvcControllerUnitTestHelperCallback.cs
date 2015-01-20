// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.AspNet.Http.Core.Collections;

namespace Microsoft.AspNet.Mvc.Core
{
	public class MvcControllerUnitTestHelperCallback
	{
		public Func<CancellationToken> OnRquestAborted
		{
			get;
			set;
		}
		public Func<FormCollection> OnRequestFormCollection
		{
			get;
			set;
		}

		public Func<string, string> OnUrlAction
		{
			get;
			set;
		}
	}
}