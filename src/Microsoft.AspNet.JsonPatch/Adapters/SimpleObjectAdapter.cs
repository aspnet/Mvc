using System;
using Microsoft.AspNet.JsonPatch.Operations;

namespace Microsoft.AspNet.JsonPatch.Adapters
{
	public class SimpleObjectAdapter<T> : IObjectAdapter<T> where T : class
	{
		public void Add(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}

		public void Copy(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}

		public void Move(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}

		public void Remove(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}

		public void Replace(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}

		public void Test(Operation<T> operation, T objectToApplyTo)
		{
			throw new NotImplementedException();
		}
	}
}