using Newtonsoft.Json;
using System;

namespace Microsoft.AspNet.JsonPatch.Operations
{
	public class OperationBase
	{
		[JsonIgnore]
		public OperationType OperationType
		{
			get
			{
				return (OperationType)Enum.Parse(typeof(OperationType), op, true);
			}
		}


		[JsonProperty("path")]
		public string path { get; set; }

		[JsonProperty("op")]
		public string op { get; set; }

		[JsonProperty("from")]
		public string from { get; set; }


		public OperationBase()
		{

		}

		public OperationBase(string op, string path, string from)
		{
			this.op = op;
			this.path = path;
			this.from = from;
		}



		public bool ShouldSerializefrom()
		{
			return (OperationType == Operations.OperationType.Move
				|| OperationType == OperationType.Copy);
		}
	}

}