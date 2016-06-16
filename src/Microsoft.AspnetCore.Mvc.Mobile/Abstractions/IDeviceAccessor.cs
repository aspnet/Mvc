namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    public interface IDeviceAccessor
    {
        IDevice Device { get; }
        IDevice Preference { get; }
    }
}