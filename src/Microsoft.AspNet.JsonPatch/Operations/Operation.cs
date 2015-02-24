using Newtonsoft.Json;
using System;

namespace Microsoft.AspNet.JsonPatch.Operations
{
	public class Operation : OperationBase
	{
		[JsonProperty("value")]
		public object value { get; set; }

		public Operation()
		{

		}


		public Operation(string op, string path, string from, object value)
			: base(op, path, from)
		{
			this.value = value;
		}

		public Operation(string op, string path, string from)
			: base(op, path, from)
		{

		}

	}
}