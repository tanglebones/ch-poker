using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CH.Poker.App.Test
{
    internal class TestsForInvalidCards : ITestCaseProvider
    {


        public IEnumerable<ITestCase> Cases
        {
            get
            {
                foreach (var input in
                    new[]
                        {
                            "Bob, 4H, 5H, 6H, 7H, 0H",
                            "Bob, 4H, 5H, 6H, IH, 7H",
                            "Bob, 4H, 5H, AceH, 6H, 7H",
                            "Bob, 4H, 1H, 5H, 6H, 7H",
                            "Bob, PH, 1H, 5H, 6H, 7H",
                            "Bob, 4H, 5H, 6H, 7H, 4$",
                            "Bob, 4S, 5C, 6D, 4R, 7H",
                            "Bob, 4S, 5C, 65, 4D, 7H",
                            "Bob, 4S, 4., 4D, 4C, 4H",
                            "Bob, 4, 4C, 4D, 4S, 4H",
                        })
                    yield return
                        new TestCase(
                            "Input with invalid cards an error: " + input,
                            new[]
                                {
                                    input
                                },
                            Output.MatchesRegex(new Regex("invalid.*?card", RegexOptions.IgnoreCase)));

                yield return new TestCase(
                    "Duplicated cards should not be allowed within a hard",
                    new[] {"Bob, 4H, 4H, 5H, 6H, 7H"},
                    Output.MatchesRegex(new Regex("duplicate.*?card", RegexOptions.IgnoreCase))
                    );
            }
        }

    }
}