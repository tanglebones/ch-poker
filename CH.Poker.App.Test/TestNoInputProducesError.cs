using System.Collections.Generic;
using System.Linq;

namespace CH.Poker.App.Test
{
    internal class TestNoInputProducesError : ITestCaseProvider
    {
        #region ITestCaseProvider Members

        public IEnumerable<ITestCase> Cases
        {
            get
            {
                yield return
                    new TestCase(
                        "No input produces error",
                        Enumerable.Empty<string>(),
                        Output.Contains("Error")
                        );
            }
        }

        #endregion
    }
}