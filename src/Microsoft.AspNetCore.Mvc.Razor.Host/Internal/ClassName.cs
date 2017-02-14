﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public static class ClassName
    {
        public static string GetClassNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return SanitizeClassName(path);
        }

        // CSharp Spec §2.4.2
        private static bool IsIdentifierStart(char character)
        {
            return char.IsLetter(character) ||
                character == '_' ||
                CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.LetterNumber;
        }

        public static bool IsIdentifierPart(char character)
        {
            return char.IsDigit(character) ||
                   IsIdentifierStart(character) ||
                   IsIdentifierPartByUnicodeCategory(character);
        }

        private static bool IsIdentifierPartByUnicodeCategory(char character)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            return category == UnicodeCategory.NonSpacingMark || // Mn
                category == UnicodeCategory.SpacingCombiningMark || // Mc
                category == UnicodeCategory.ConnectorPunctuation || // Pc
                category == UnicodeCategory.Format; // Cf
        }

        private static string SanitizeClassName(string inputName)
        {
            if (!IsIdentifierStart(inputName[0]) && IsIdentifierPart(inputName[0]))
            {
                inputName = "_" + inputName;
            }

            var builder = new InplaceStringBuilder(inputName.Length);
            for (var i = 0; i < inputName.Length; i++)
            {
                var ch = inputName[i];
                builder.Append(IsIdentifierPart(ch) ? ch : '_');
            }

            return builder.ToString();
        }
    }
}
