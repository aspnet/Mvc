using System;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    public interface IOptionActivator<TOption>
    {
        TOption CreateInstance(Type optionType);
    }
}