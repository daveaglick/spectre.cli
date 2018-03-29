﻿using System;
using System.Linq;

namespace Spectre.CommandLine.Tests.Data
{
    public sealed class ThrowingCommand : Command<ThrowingCommandSettings>
    {
        public override int Execute(ThrowingCommandSettings settings, ILookup<string, string> remaining)
        {
            throw new InvalidOperationException("W00t?");
        }
    }
}