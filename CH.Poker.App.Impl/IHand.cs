using System.Collections.Generic;

namespace CH.Poker.App.Impl
{
    internal interface IHand
    {
        int Score { get; set; }
        string Owner { get; }
        IEnumerable<string> Cards { get; }
    }
}