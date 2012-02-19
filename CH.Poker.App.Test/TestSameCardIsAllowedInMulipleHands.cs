using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal class TestSameCardIsAllowedInMulipleHands
    {
        public IEnumerable<ITestCase> Cases
        {
            get
            {
                yield return
                    new TestCase(
                        "Having one single hand is valid and doesn't produce an error",
                        new[]
                            {
                                "Alice, ah, 2d, 4d, 3d, 6d",
                                "Bob, ah, 2d, 2c, 2h, 5d",
                                "Eve, ad, 2d, 4d, 3d, 5d",
                            },
                        new[]
                            {
                                "Eve"
                            }
                        );
            }
        }
    }
}