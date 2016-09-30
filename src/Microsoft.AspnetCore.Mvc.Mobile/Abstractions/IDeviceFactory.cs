namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    public interface IDeviceFactory
    {
        IDevice Normal();
        IDevice Mobile();
        IDevice Tablet();
        IDevice Other(string code);
    }
}