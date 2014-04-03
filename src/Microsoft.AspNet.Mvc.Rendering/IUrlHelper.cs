namespace Microsoft.AspNet.Mvc
{
    public interface IUrlHelper
    {
        string Action(string action, string controller, object values, string protocol, string host, string fragment);

        string RouteUrl(object values, string protocol, string host, string fragment);
    }
}
