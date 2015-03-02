using System;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
	internal class ConversionResult
	{
		public bool CanBeConverted { get; private set; }
		public object ConvertedInstance { get; private set; }


		public ConversionResult(bool canBeConverted, object convertedInstance)
		{
			CanBeConverted = canBeConverted;
			ConvertedInstance = convertedInstance;

		}
	}
}