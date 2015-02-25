using Microsoft.AspNet.JsonPatch.Operations;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.JsonPatch
{
	public interface IJsonPatchDocument
	{
		List<Operation> GetOperations();
	}
}