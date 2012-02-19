using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal interface ITestCaseProvider
    {
        IEnumerable<ITestCase> Cases { get; }
    }
}