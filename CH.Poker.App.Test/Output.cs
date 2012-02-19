using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CH.Poker.App.Test
{
    internal static class Output
    {
        public static readonly Func<string, Func<IEnumerable<string>, string>> Contains =
            what =>
            lines =>
            lines.Any(line => line.ToUpperInvariant().Contains(what.ToUpperInvariant()))
                ? null
                : "Expected output to contain: " + what;

        public static readonly Func<Regex, Func<IEnumerable<string>, string>> MatchesRegex =
            regex =>
            lines =>
            lines.Any(regex.IsMatch)
                ? null
                : "Expected output to match: " + regex.ToString();
    }
}