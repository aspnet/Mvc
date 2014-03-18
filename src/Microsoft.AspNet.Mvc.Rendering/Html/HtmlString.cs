// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlString
    {
        private readonly string _input;

        public HtmlString(string input)
        {
            _input = input;
        }

        public override string ToString()
        {
            return _input;
        }
    }
}
