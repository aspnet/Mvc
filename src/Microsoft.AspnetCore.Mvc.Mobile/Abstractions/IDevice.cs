namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    public interface IDevice
    {
        bool IsMobile { get; }
        bool IsTablet { get; }
        bool IsNormal { get; }
        string DeviceCode { get; }
    }
}