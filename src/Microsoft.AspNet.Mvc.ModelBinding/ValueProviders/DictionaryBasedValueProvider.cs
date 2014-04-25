using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class DictionaryBasedValueProvider : IValueProvider
    {
        private readonly IDictionary<string, object> _values;

        public DictionaryBasedValueProvider(IDictionary<string, object> values)
        {
            _values = values;
        }

        public Task<bool> ContainsPrefixAsync(string key)
        {
            return Task.FromResult(_values.ContainsKey(key));
        }

        public Task<ValueProviderResult> GetValueAsync([NotNull] string key)
        {
            object value;
            ValueProviderResult result = null;
            if (_values.TryGetValue(key, out value))
            {
                result = new ValueProviderResult(value, value.ToString(), CultureInfo.InvariantCulture);
            }
            
            return Task.FromResult(result);
        }
    }
}
