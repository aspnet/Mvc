
namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationResult
    {
        private string _memberName;
        private string _message;

        public string MemberName
        {
            get { return _memberName ?? string.Empty; }
            set { _memberName = value; }
        }

        public string Message
        {
            get { return _message ?? string.Empty; }
            set { _message = value; }
        }
    }
}
