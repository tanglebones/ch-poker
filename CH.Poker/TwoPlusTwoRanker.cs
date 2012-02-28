using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CH.Poker
{
    // The TwoPlusTwo ranker uses a normal naive ranker to build a series lookup table that can be used to get the ranking of a card in 5 integer array lookups.

    public class TwoPlusTwoRanker : IRanker
    {
        private readonly IRanker _ranker;
        private int[] _table;

        public TwoPlusTwoRanker() : this(new SimpleRanker())
        {
        }

        public TwoPlusTwoRanker(IRanker ranker)
        {
            _ranker = ranker;
            CreateTable();
        }

        private class TableInfo
        {
            private readonly int _hashCode;
            public char Suit { get; private set; }
            public char[] Ranks { get; private set; }
            public int Score { get; set; }

            public TableInfo(char suit, char[] ranks)
            {
                Suit = suit;
                Ranks = ranks;
                Score = int.MaxValue;
                _hashCode = new string(new[] {Suit}.Concat(ranks).ToArray()).GetHashCode();
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            static public bool operator==(TableInfo lhs, TableInfo rhs)
            {
                if (lhs == null || rhs == null) return false;
                return lhs._hashCode == rhs._hashCode;
            }
            static public bool operator!=(TableInfo lhs, TableInfo rhs)
            {
                if (lhs == null && rhs == null) return false;
                if (lhs == null || rhs == null) return true;
                return lhs._hashCode != rhs._hashCode;                
            }

            public override bool Equals(object obj)
            {
                return _hashCode == obj.GetHashCode();
            }
        }


        private void CreateTable()
        {
            var map = new Dictionary<Tuple<TableInfo, char, char>, TableInfo>(); // state + rank + quit -> new state
            var levels =
                new[]
                    {
                        new HashSet<TableInfo>(),
                        new HashSet<TableInfo>(),
                        new HashSet<TableInfo>(),
                        new HashSet<TableInfo>(),
                        new HashSet<TableInfo>(),
                    };

            var suits = new[] {'H', 'D', 'C', 'S'};
            var ranks = new[] {'2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'};

            // first card
            foreach (var rank in ranks)
            {
                foreach (var suit in suits)
                {
                    levels[0].Add(new TableInfo(suit, new[] {rank}));
                }
            }

            foreach (var level in Enumerable.Range(0, levels.Length - 1))
            {
                var nextLevel = level + 1;
                foreach (var state in levels[level])
                {
                    foreach (var rank in ranks)
                    {
                        foreach (var suit in suits)
                        {
                            var newSuit = (state.Suit != suit) ? 'X' : state.Suit;
                            var newRanks = new[] {rank}.Concat(state.Ranks).OrderBy(r => r).ToArray();
                            var newState = new TableInfo(newSuit, newRanks);
                            var newLookup = Tuple.Create(new TableInfo(state.Suit, state.Ranks), rank, suit);
                            map[newLookup] = newState;
                            levels[nextLevel].Add(newState);
                        }
                    }
                }
            }

            foreach (var state in levels[levels.Length - 1])
            {
                if (state.Suit == 'X')
                {
                    var cardScores = new List<int>();
                    for (var index = 0; index < state.Ranks.Length; index++)
                    {
                        var rank = state.Ranks[index];
                        var suit = 'H';
                        if (index == 0) suit = 'D';
                        cardScores.Add(_ranker.ScoreCard(new string(new[]{rank,suit})));
                    }
                    state.Score = _ranker.ScoreHand(cardScores);
                }
                else
                {
                    state.Score =
                        _ranker.ScoreHand(state.Ranks.Select(rank => _ranker.ScoreCard(new string(new[] {rank, 'H'}))));
                }
            }

            var arraySize = levels.Aggregate(0, (c, x) => c + x.Count);
            _table = new int[arraySize];

            throw new NotImplementedException();
        }

        public int ScoreCard(string card)
        {
            return _ranker.ScoreCard(card);
        }

        public int ScoreHand(IEnumerable<int> cards)
        {
            Debug.Assert(cards != null);
            var cardsAsArray = cards.ToArray();
            Debug.Assert(cardsAsArray.Length == 5);
            Debug.Assert(cardsAsArray.All(card => ((card >= 0) && (card <= 51))));
            Debug.Assert(_table != null);

            return
                _table[
                    _table[_table[_table[_table[cardsAsArray[0]] + cardsAsArray[1]] + cardsAsArray[2]] + cardsAsArray[3]
                        ] + cardsAsArray[4]];
        }
    }
}