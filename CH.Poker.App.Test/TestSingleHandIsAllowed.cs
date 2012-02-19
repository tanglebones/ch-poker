using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal class TestSingleHandIsAllowed
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
                                "Single, ah, 2d, 4d, 3d, 5d"
                            },
                        new[]
                            {
                                "Single"
                            }
                        );
            }
        }
    }
}