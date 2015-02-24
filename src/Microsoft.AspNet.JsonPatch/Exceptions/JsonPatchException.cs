using System;

namespace Microsoft.AspNet.JsonPatch.Exceptions
{
	public class JsonPatchException : Exception
	{
		public new Exception InnerException { get; internal set; }

		public object AffectedObject { get; private set; }

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

		public JsonPatchException(string message, Exception innerException)
		{
			_message = message;
			InnerException = innerException;
		}

	}
}