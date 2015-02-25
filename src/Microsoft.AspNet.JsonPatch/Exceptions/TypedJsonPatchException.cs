using Microsoft.AspNet.JsonPatch.Operations;
using System;

namespace Microsoft.AspNet.JsonPatch.Exceptions
{
	public class JsonPatchException<T> : JsonPatchException where T : class
	{
		public Operation<T> FailedOperation { get; private set; }
		public new T AffectedObject { get; private set; }

		private string _message = "";
		public override string Message
		{
			get
			{
				return _message;
			}

		}

		public JsonPatchException()
		{

		}

		public JsonPatchException(Operation<T> operation, string message, T affectedObject)
		{
			FailedOperation = operation;
			_message = message;
			AffectedObject = affectedObject;
		}

		public JsonPatchException(Operation<T> operation, string message, T affectedObject, Exception innerException)
			: this(operation, message, affectedObject)
		{
			InnerException = innerException;
		}
	}
}