// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal class CommandOption
    {
        public CommandOption(string template, CommandOptionType optionType)
        {
            Template = template;
            OptionType = optionType;
            Values = new List<string>();

            foreach (var part in Template.Split(new[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("--"))
                {
                    LongName = part.Substring(2);
                }
                else if (part.StartsWith("-"))
                {
                    var optName = part.Substring(1);

                    // If there is only one char and it is not an English letter, it is a symbol option (e.g. "-?")
                    if (optName.Length == 1 && !IsEnglishLetter(optName[0]))
                    {
                        SymbolName = optName;
                    }
                    else
                    {
                        ShortName = optName;
                    }
                }
                else if (part.StartsWith("<") && part.EndsWith(">"))
                {
                    ValueName = part.Substring(1, part.Length - 2);
                }
                else if (optionType == CommandOptionType.MultipleValue && part.StartsWith("<") && part.EndsWith(">..."))
                {
                    ValueName = part.Substring(1, part.Length - 5);
                }
                else
                {
                    throw new ArgumentException($"Invalid template pattern '{template}'", nameof(template));
                }
            }

            if (string.IsNullOrEmpty(LongName) && string.IsNullOrEmpty(ShortName) && string.IsNullOrEmpty(SymbolName))
            {
                throw new ArgumentException($"Invalid template pattern '{template}'", nameof(template));
            }
        }

        public string Template { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string SymbolName { get; set; }
        public string ValueName { get; set; }
        public string Description { get; set; }
        public List<string> Values { get; private set; }
        public bool? BoolValue { get; private set; }
        public CommandOptionType OptionType { get; private set; }

        public bool TryParse(string value)
        {
            switch (OptionType)
            {
                case CommandOptionType.MultipleValue:
                    Values.Add(value);
                    break;
                case CommandOptionType.SingleValue:
                    if (Values.Any())
                    {
                        return false;
                    }
                    Values.Add(value);
                    break;
                case CommandOptionType.BoolValue:
                    if (Values.Any())
                    {
                        return false;
                    }

                    if (value == null)
                    {
                        // add null to indicate that the option was present, but had no value
                        Values.Add(null);
                        BoolValue = true;
                    }
                    else
                    {
                        if (!bool.TryParse(value, out var boolValue))
                        {
                            return false;
                        }

                        Values.Add(value);
                        BoolValue = boolValue;
                    }
                    break;
                case CommandOptionType.NoValue:
                    if (value != null)
                    {
                        return false;
                    }
                    // Add a value to indicate that this option was specified
                    Values.Add("on");
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool HasValue() => Values.Any();

        public string Value() => HasValue() ? Values[0] : null;

        private static bool IsEnglishLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
