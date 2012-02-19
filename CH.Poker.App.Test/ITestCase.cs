using System;
using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    public interface ITestCase
    {
        string TestName { get; }
        IEnumerable<string> InputLines { get; }
        Func<IEnumerable<string>, string> Checker { get; }
    }
}