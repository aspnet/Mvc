namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    public class DefaultDeviceFactory : IDeviceFactory
    {
        private readonly DeviceOptions _options;

        public DefaultDeviceFactory(DeviceOptions options)
        {
            _options = options;
        }

        public IDevice Normal() => new LiteDevice(DeviceType.Normal, string.Empty);
        public IDevice Mobile() => new LiteDevice(DeviceType.Mobile, _options.MobileCode);
        public IDevice Tablet() => new LiteDevice(DeviceType.Tablet, _options.TabletCode);
        public IDevice Other(string code) => new LiteDevice(DeviceType.Other, code);
    }
}