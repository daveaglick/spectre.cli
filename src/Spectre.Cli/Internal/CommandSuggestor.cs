using System;
using System.Linq;
using Spectre.Cli.Internal.Modelling;

namespace Spectre.Cli.Internal
{
    internal static class CommandSuggestor
    {
        private const float _smallestDistance = 2f;

        public static CommandInfo Suggest(CommandModel model, CommandInfo command, string name)
        {
            var result = (CommandInfo)null;

            var container = command ?? (ICommandContainer)model;
            if (command?.IsDefaultCommand ?? false)
            {
                // Default commands have no children,
                // so use the root commands here.
                container = model;
            }

            var score = float.MaxValue;
            foreach (var child in container.Commands)
            {
                var temp = Score(child.Name, name);
                if (temp < score)
                {
                    score = temp;
                    result = child;
                }
            }

            if (score <= _smallestDistance)
            {
                return result;
            }

            return null;
        }

        private static float Score(string source, string target)
        {
            source = source.ToLowerInvariant();
            target = target.ToLowerInvariant();

            var n = source.Length;
            var m = target.Length;

            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }

            int[,] d = new int[n + 1, m + 1];
            Enumerable.Range(0, n + 1).ToList().ForEach(i => d[i, 0] = i);
            Enumerable.Range(0, m + 1).ToList().ForEach(i => d[0, i] = i);

            for (var sourceIndex = 1; sourceIndex <= n; sourceIndex++)
            {
                for (var targetIndex = 1; targetIndex <= m; targetIndex++)
                {
                    var cost = (target[targetIndex - 1] == source[sourceIndex - 1]) ? 0 : 1;
                    d[sourceIndex, targetIndex] = Math.Min(
                        Math.Min(d[sourceIndex - 1, targetIndex] + 1, d[sourceIndex, targetIndex - 1] + 1),
                        d[sourceIndex - 1, targetIndex - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}
