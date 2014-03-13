﻿
namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationResult
    {
        public ModelValidationResult(string memberName, string message)
        {
            MemberName = memberName ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string MemberName { get; private set; }

        public string Message { get; private set; }
    }
}
