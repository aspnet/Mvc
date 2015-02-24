using System;

namespace Microsoft.AspNet.JsonPatch.Adapters
{
	public interface IObjectAdapter<T>
	  where T : class
	{
		void Add(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
		void Copy(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
		void Move(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
		void Remove(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
		void Replace(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
		void Test(Microsoft.AspNet.JsonPatch.Operations.Operation<T> operation, T objectToApplyTo);
	}
}