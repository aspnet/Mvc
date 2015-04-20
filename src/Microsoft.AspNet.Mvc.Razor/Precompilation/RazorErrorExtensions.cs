﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    public static class RazorErrorExtensions
    {
        public static Diagnostic ToDiagnostics([NotNull] this RazorError error, [NotNull] string filePath)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "Razor",
                title: "Razor parsing error",
                messageFormat: error.Message.Replace("{", "{{").Replace("}", "}}"),
                category: "Razor.Parser",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            var location = error.Location;
            if (location.Equals(SourceLocation.Undefined))
            {
                location = SourceLocation.Zero;
            }
            var length = Math.Max(0, error.Length);

            var textSpan = new TextSpan(location.AbsoluteIndex, length);
            var linePositionStart = new LinePosition(location.LineIndex, location.CharacterIndex);
            var linePositionEnd = new LinePosition(location.LineIndex, location.CharacterIndex + length);
            var linePositionSpan = new LinePositionSpan(linePositionStart, linePositionEnd);
            return Diagnostic.Create(descriptor, Location.Create(filePath, textSpan, linePositionSpan));
        }
    }
}