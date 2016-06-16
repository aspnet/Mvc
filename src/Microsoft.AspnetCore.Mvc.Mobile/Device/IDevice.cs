namespace Microsoft.AspnetCore.Mvc.Mobile.Device
{
    public interface IDevice
    {
        bool IsMobile { get; }
        bool IsTablet { get; }
        bool IsNormal { get; }
        string DeviceCode { get; }
    }
}