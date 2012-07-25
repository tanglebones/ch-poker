using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CH.Poker.App.Test
{
    internal class TestNonUniqueOwnerNamesCausesAnError : ITestCaseProvider
    {
        #region ITestCaseProvider Members

        public IEnumerable<ITestCase> Cases
        {
            get
            {
                foreach (var input in
                    new[]
                        {
                            new[] {"Bob, 4H, 6H, 8H, 2H, KH", "Bob, 4H, 6H, 8H, 2H, KH",},
                            new[] {" Bob, 4H, 6H, 8H, 2H, KH", " Bob , 4H, 6H, 8H, 2H, KH",},
                            new[] {"Bob , 4H, 6H, 8H, 2H, KH", " Bob, 4H, 6H, 8H, 2H, KH",},
                        })
                    yield return
                        new TestCase(
                            "Test that non unique owner names cause an error: " + string.Join(" / ", input),
                            input,
                            Output.MatchesRegex(new Regex("owner.*?already specified", RegexOptions.IgnoreCase)
                                )
                            );
            }
        }

        #endregion
    }
}