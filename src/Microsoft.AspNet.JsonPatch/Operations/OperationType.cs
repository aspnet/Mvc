using System;

namespace Microsoft.AspNet.JsonPatch.Operations
{
	public enum OperationType
	{
		Add,
		Remove,
		Replace,
		Move,
		Copy,
		Test
	}
}