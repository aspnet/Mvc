using System;
using System.Collections.Generic;

namespace MvcSandbox
{
    public class Translations
    {
        private readonly Dictionary<string, string> _invariantToTranslation;
        private readonly Dictionary<string, string> _translationToInvariant;

        public Translations(IEnumerable<(string invariant, string translated)> translations)
        {
            _invariantToTranslation = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _translationToInvariant = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var term in translations)
            {
                _invariantToTranslation[term.invariant] = term.translated;
                _translationToInvariant[term.translated] = term.invariant;
            }
        }

        public string LookupInvariant(string translated)
        {
            if (translated == null)
            {
                return null;
            }

            _translationToInvariant.TryGetValue(translated, out var invariant);
            return invariant ?? translated;
        }


        public string Translate(string invariant)
        {
            if (invariant == null)
            {
                return null;
            }

            _invariantToTranslation.TryGetValue(invariant, out var translation);
            return translation ?? invariant;
        }
    }
}
