using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CH.Poker
{
    // The TwoPlusTwo ranker uses a normal naive ranker to build a series lookup table that can be used to get the ranking of a card in 5 integer array lookups.

    public class TwoPlusTwoRanker : IRanker
    {
        private const int NoSuit = -1;
        private const int AnySuit = -2;
        private const int ScoreOffset = 100000000;
        private static readonly IDictionary<int, string> ScoreToCard = new Dictionary<int, string>();

        private static readonly IDictionary<char, int> SuitToScore =
            new Dictionary<char, int>
                {
                    {'H', 0},
                    {'D', 1},
                    {'C', 2},
                    {'S', 3},
                    {'X', NoSuit},
                    {'*', AnySuit}
                };

        private static readonly IDictionary<char, int> RankToScore =
            new Dictionary<char, int>
                {
                    // low ace
                    {'a', 0},
                    {'2', 1},
                    {'3', 2},
                    {'4', 3},
                    {'5', 4},
                    {'6', 5},
                    {'7', 6},
                    {'8', 7},
                    {'9', 8},
                    {'T', 9},
                    {'J', 10},
                    {'Q', 11},
                    {'K', 12},
                    {'A', 13},
                };

        private static readonly IDictionary<int, char> ScoreToSuit = new Dictionary<int, char>();
        private static readonly IDictionary<int, char> ScoreToRank = new Dictionary<int, char>();

        private static readonly int[] CardSuit = new int[52];
        private static readonly int[] CardRank = new int[52];

        private static readonly IDictionary<string, int> CardToScore =
            new Dictionary<string, int>
                {
                    {"2H", 0},
                    {"2D", 1},
                    {"2C", 2},
                    {"2S", 3},
                    {"3H", 4},
                    {"3D", 5},
                    {"3C", 6},
                    {"3S", 7},
                    {"4H", 8},
                    {"4D", 9},
                    {"4C", 10},
                    {"4S", 11},
                    {"5H", 12},
                    {"5D", 13},
                    {"5C", 14},
                    {"5S", 15},
                    {"6H", 16},
                    {"6D", 17},
                    {"6C", 18},
                    {"6S", 19},
                    {"7H", 20},
                    {"7D", 21},
                    {"7C", 22},
                    {"7S", 23},
                    {"8H", 24},
                    {"8D", 25},
                    {"8C", 26},
                    {"8S", 27},
                    {"9H", 28},
                    {"9D", 29},
                    {"9C", 30},
                    {"9S", 31},
                    {"TH", 32},
                    {"TD", 33},
                    {"TC", 34},
                    {"TS", 35},
                    {"JH", 36},
                    {"JD", 37},
                    {"JC", 38},
                    {"JS", 39},
                    {"QH", 40},
                    {"QD", 41},
                    {"QC", 42},
                    {"QS", 43},
                    {"KH", 44},
                    {"KD", 45},
                    {"KC", 46},
                    {"KS", 47},
                    {"AH", 48},
                    {"AD", 49},
                    {"AC", 50},
                    {"AS", 51},
                };

        private static readonly int[] Cards;

        private readonly IRanker _ranker;
        private int[] _table;

        static TwoPlusTwoRanker()
        {
            foreach (var kvp in CardToScore)
            {
                ScoreToCard[kvp.Value] = kvp.Key;
                CardRank[kvp.Value] = RankToScore[kvp.Key[0]];
                CardSuit[kvp.Value] = SuitToScore[kvp.Key[1]];
            }
            foreach (var kvp in RankToScore)
            {
                ScoreToRank[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in SuitToScore)
            {
                ScoreToSuit[kvp.Value] = kvp.Key;
            }
            Cards = Enumerable.Range(0, CardToScore.Count).ToArray();
        }

        public TwoPlusTwoRanker() : this(new SimpleRanker())
        {
        }

        public TwoPlusTwoRanker(IRanker ranker)
        {
            _ranker = ranker;
            CreateTable();
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

            var hop1 = _table[cardsAsArray[0]];
            Debug.Assert(hop1 > 0 && hop1 < _table.Length - 52);
            var hop2 = _table[hop1 + cardsAsArray[1]];
            Debug.Assert(hop2 > 0 && hop2 < _table.Length - 52);
            var hop3 = _table[hop2 + cardsAsArray[2]];
            Debug.Assert(hop3 > 0 && hop3 < _table.Length - 52);
            var hop4 = _table[hop3 + cardsAsArray[3]];
            Debug.Assert(hop4 > 0 && hop4 < _table.Length - 52);
            var hop5 = _table[hop4 + cardsAsArray[4]];
            Debug.Assert(hop5 > 0 && hop5 < _table.Length);
            var score = _table[hop5];
            Debug.Assert(score >= ScoreOffset);

            return score;
        }


        private void CreateTable()
        {
            var map = new Dictionary<string, TableInfo>(); // state + card -> new state

            var root = new TableInfo();
            map[root.ToString()] = root;

            IList<TableInfo> q = new List<TableInfo>
                {
                    root
                };

            var location = Cards.Length;

            q = AddLevel(map, q, ref location);
            q = AddLevel(map, q, ref location);
            q = AddLevel(map, q, ref location);
            q = AddLevel(map, q, ref location);
            AddLevel(map, q, ref location, true);

            Debug.Assert(location < ScoreOffset);

            _table = new int[location];
            for (var i = 0; i < _table.Length; ++i)
            {
                _table[i] = -1;
            }

            foreach (var entry in map.Values)
            {
                var baseOffset = entry.ArrayLocation;
                if (entry.Score != -1)
                {
                    Debug.Assert(_table[baseOffset] < 0);
                    _table[baseOffset] = entry.Score + ScoreOffset;
                }
                else
                {
                    for (var i = 0; i < entry.Exits.Length; ++i)
                    {
                        var arrayLocation = baseOffset + i;
                        Debug.Assert(_table[arrayLocation] < 0);
                        _table[arrayLocation] = entry.Exits[i];
                    }
                }
            }
        }

        private IList<TableInfo> AddLevel(IDictionary<string, TableInfo> map, IList<TableInfo> tableInfos,
                                          ref int location, bool end = false)
        {
            var newQ = new List<TableInfo>();

            foreach (var entry in tableInfos)
            {
                // for each card added to previous state
                foreach (var card in Cards)
                {
                    var suit = CardSuit[card];
                    var rank = CardRank[card];

                    // compute new state
                    var newSuit = (entry.Suit == AnySuit) ? suit : (entry.Suit != suit) ? NoSuit : suit;
                    var newRanks = new[] {rank}.Concat(entry.Ranks).OrderBy(r => r).ToArray();

                    var newState = new TableInfo(newRanks, newSuit);
                    var newLookup = newState.ToString();

                    if (map.ContainsKey(newLookup)) continue;

                    newState.ComputeScore(_ranker);
                    map[newLookup] = newState;
                    newQ.Add(newState);
                }
            }

            newQ.Sort(ByScoreOrString);

            foreach (var newEntry in newQ)
            {
                newEntry.ArrayLocation = location;
                location += end ? 1 : Cards.Length;
            }

            foreach (var entry in tableInfos)
            {
                entry.Exits = new int[Cards.Length];

                // for each card added to previous state
                foreach (var card in Cards)
                {
                    var suit = CardSuit[card];
                    var rank = CardRank[card];

                    // compute new state
                    var newSuit = (entry.Suit == AnySuit) ? suit : (entry.Suit != suit) ? NoSuit : suit;
                    var newRanks = new[] {rank}.Concat(entry.Ranks).OrderBy(r => r).ToArray();

                    var newLookup = new TableInfo(newRanks, newSuit).ToString();

                    TableInfo newState;
                    if (map.TryGetValue(newLookup, out newState))
                    {
                        entry.Exits[card] = newState.ArrayLocation;
                    }
                }
            }

            return newQ;
        }

        private static int ByScoreOrString(TableInfo x, TableInfo y)
        {
            if (x.Score == -1 && y.Score == -1)
                return StringComparer.InvariantCultureIgnoreCase.Compare(x.ToString(), y.ToString());
            return x.Score - y.Score;
        }

        private class TableInfo
        {
            private string _toString;

            public TableInfo()
            {
                _toString = string.Empty;
                Score = -1;
                Suit = AnySuit;
                Ranks = new int[] {};
                ArrayLocation = 0;
            }

            public TableInfo(int[] ranks, int suit)
            {
                Suit = suit;
                Ranks = ranks;
                Score = -1;
                ArrayLocation = -1;
                Exits = null;
            }

            public int Suit { get; private set; }
            public int[] Ranks { get; private set; }
            public int Score { get; private set; }
            public int ArrayLocation { get; set; }
            public int[] Exits { get; set; }

            public override string ToString()
            {
                return _toString ??
                       (_toString =
                        ((Suit == -1) ? 'X' : ScoreToSuit[Suit]) +
                        String.Join(string.Empty, Ranks.Select(r => ScoreToRank[r])));
            }

            public void ComputeScore(IRanker ranker)
            {
                if (Ranks.Length != 5)
                    return;

                if (Suit == -1)
                {
                    var cardScores = new List<int>();
                    for (var index = 0; index < Ranks.Length; index++)
                    {
                        var rank = Ranks[index];
                        var suit = 'D';
                        if (index == 0)
                            suit = 'H'; // may result in weird hands like 2D 3H 3H ...
                        // score mixed suits (non-flush) by using a different suit for the first card
                        cardScores.Add(ranker.ScoreCard(new string(new[] {ScoreToRank[rank], suit})));
                    }
                    Score = ranker.ScoreHand(cardScores);
                }
                else
                {
                    // score using the same suit for all cards (flush)
                    Score =
                        ranker.ScoreHand(
                            Ranks.Select(rank => ranker.ScoreCard(new string(new[] {ScoreToRank[rank], 'H'}))));
                }
            }
        }
    }
}