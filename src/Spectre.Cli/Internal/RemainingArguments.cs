﻿using System.Collections.Generic;
using System.Linq;

namespace Spectre.Cli.Internal
{
    internal sealed class RemainingArguments : IRemainingArguments
    {
        public IReadOnlyList<string> Raw { get; }
        public ILookup<string, string> Parsed { get; }

        public RemainingArguments(
            ILookup<string, string> remaining,
            IReadOnlyList<string> raw)
        {
            Parsed = remaining;
            Raw = raw;
        }
    }
}
