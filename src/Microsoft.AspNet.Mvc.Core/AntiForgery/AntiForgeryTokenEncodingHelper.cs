// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Microsoft.AspNet.Mvc
{
    internal static class AntiForgeryTokenEncodingHelper
    { 
        public static string UrlTokenEncode(byte[] input)
        {
            var base64String = Convert.ToBase64String(input);
            if (string.IsNullOrEmpty(base64String))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < base64String.Length; i++)
            {
                switch (base64String[i])
                {
                    case '+':
                        sb.Append('-');
                        break;
                    case '/':
                        sb.Append('_');
                        break;
                    case '=':
                        sb.Append('.');
                        break;
                    default:
                        sb.Append(base64String[i]);
                        break;
                }
            }

            return sb.ToString();
        }

        public static byte[] UrlTokenDecode(string input)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '-':
                        sb.Append('+');
                        break;
                    case '_':
                        sb.Append('/');
                        break;
                    case '.':
                        sb.Append('=');
                        break;
                    default:
                        sb.Append(input[i]);
                        break;
                }
            }

            return Convert.FromBase64String(sb.ToString());
        }
    }
}