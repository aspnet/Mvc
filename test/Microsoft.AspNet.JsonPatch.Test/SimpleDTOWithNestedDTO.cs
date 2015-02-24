using System;

namespace Microsoft.AspNet.JsonPatch.Test
{

	public class SimpleDTOWithNestedDTO
	{
		public int IntegerValue { get; set; }

		public NestedDTO NestedDTO { get; set; }

		public SimpleDTO SimpleDTO { get; set; }

		public SimpleDTOWithNestedDTO()
		{
			this.NestedDTO = new NestedDTO();
			this.SimpleDTO = new SimpleDTO();
		}
	}
}