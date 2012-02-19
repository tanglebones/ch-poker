using System.Collections.Generic;

namespace CH.Poker.App.Impl
{
    internal sealed class Hand : IHand
    {
        private int _score = int.MaxValue;
        private readonly string _owner;
        private readonly IEnumerable<string> _cards;

        public Hand(string owner, IEnumerable<string> cards)
        {
            _owner = owner;
            _cards = cards;
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public string Owner
        {
            get { return _owner; }
        }

        public IEnumerable<string> Cards
        {
            get { return _cards; }
        }
    }
}