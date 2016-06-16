namespace Microsoft.AspnetCore.Mvc.Mobile
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using AspNetCore.Mvc.Razor;
    using AspNetCore.Mvc.Razor.Internal;

    public class DeviceViewLocationExpander : IViewLocationExpander
    {
        private const string ValueKey = "device";
        private readonly ISitePreferenceRepository _deviceResolver;
        private readonly DeviceOptions _options;

        /// <summary>
        /// Instantiates a new <see cref="DefaultTagHelperActivator" /> instance.
        /// </summary>
        /// <param name="options">The <see cref="DeviceOptions" />.</param>
        /// <param name="deviceResolver">The device resolver.</param>
        public DeviceViewLocationExpander(DeviceOptions options, ISitePreferenceRepository deviceResolver)
        {
            _options = options;
            _deviceResolver = deviceResolver;
        }

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Values[ValueKey] =
                _deviceResolver.LoadPreference(context.ActionContext.HttpContext)?.DeviceCode;
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (viewLocations == null)
            {
                throw new ArgumentNullException(nameof(viewLocations));
            }

            string value;
            context.Values.TryGetValue(ValueKey, out value);

            if (!string.IsNullOrEmpty(value))
            {
                return ExpandViewLocationsCore(viewLocations, value);
            }

            return viewLocations;
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations, string deviceCode)
        {
            foreach (var location in viewLocations)
            {
                if (_options.Format == DeviceLocationExpanderFormat.SubFolder)
                {
                    yield return location.Replace("{0}", deviceCode + "/{0}");
                }
                else
                {
                    yield return location.Replace("{0}", "{0}." + deviceCode);
                }


                yield return location;
            }
        }
    }

    public enum DeviceLocationExpanderFormat
    {
        Suffix,
        SubFolder
    }
}