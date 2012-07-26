using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal class TestsForIncorrectNumberOfFields : ITestCaseProvider
    {


        public IEnumerable<ITestCase> Cases
        {
            get
            {
                foreach (var input in
                    new[]
                        {
                            new[] {"Bob, 4H"},
                            new[] {" Fred, Bob , Wrong"},
                            new[] {" 4H, 6H, 8H, 2H, KH"},
                            new[] {" 4H 6H 8H 2H KH"},
                            new[] {"Bob 4H 6H 8H 2H KH"},
                            new[] {"Bob, 4H, 6H, 8H, 2H, KH", "Bob 4H 6H 8H 2H KH"},
                        })
                    yield return
                        new TestCase(
                            "Input with incorrect number of fields produces an error: " + string.Join(" / ", input),
                            input,
                            Output.Contains("exactly 6 fields"));
            }
        }

    }
}