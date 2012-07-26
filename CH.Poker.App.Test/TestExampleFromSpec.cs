using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal class TestExampleFromSpec : ITestCaseProvider
    {


        public IEnumerable<ITestCase> Cases
        {
            get
            {
                yield return
                    new TestCase(
                        "Example from specification",
                        new[]
                            {
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                                "Bob, 3C, 3D, 3S, 8C, 10D",
                                "Sally, AC, 10C, 5C, 2S, 2C",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );
            }
        }

    }
}