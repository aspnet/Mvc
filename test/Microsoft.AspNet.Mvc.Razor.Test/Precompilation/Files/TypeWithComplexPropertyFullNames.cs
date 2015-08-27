using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public class TypeWithComplexPropertyFullNames
    {
        public int Property1 { get; set; }

        public int[] Property2 { get; set; }

        public List<long> Property3 { get; set; }

        public List<Tuple<string, DateTimeOffset>> Property4 { get; }

        public IReadOnlyDictionary<ILookup<string, dynamic>, IEnumerable<Comparer<byte[]>>> Property5 { get; }
    }
}
