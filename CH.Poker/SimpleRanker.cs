using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CH.Poker
{
    public sealed class SimpleRanker : IRanker
    {
        private static readonly IDictionary<int, string> ScoreToCard = new Dictionary<int, string>();

        private static readonly IDictionary<char, int> SuitToScore =
            new Dictionary<char, int>
                {
                    {'H', 0},
                    {'D', 1},
                    {'C', 2},
                    {'S', 3},
                };

        private static readonly IDictionary<char, int> RankToScore =
            new Dictionary<char, int>
                {
                    {'a', 13},
                    // low ace
                    {'2', 12},
                    {'3', 11},
                    {'4', 10},
                    {'5', 9},
                    {'6', 8},
                    {'7', 7},
                    {'8', 6},
                    {'9', 5},
                    {'T', 4},
                    {'J', 3},
                    {'Q', 2},
                    {'K', 1},
                    {'A', 0},
                };

        private static readonly int[] CardSuit = new int[52];
        private static readonly int[] CardRank = new int[52];

        // Hand Orders:
        // 0 - 5K
        // 1 - Straight Flush
        // 2 - 4K
        // 3 - FH
        // 4 - Flush
        // 5 - Striaght
        // 6 - 3K
        // 7 - 2P
        // 8 - 1P
        // 9 - HC

        private static readonly IDictionary<string, int> CardToScore =
            new Dictionary<string, int>
                {
                    {"2H", 51},
                    {"2D", 50},
                    {"2C", 49},
                    {"2S", 48},
                    {"3H", 47},
                    {"3D", 46},
                    {"3C", 45},
                    {"3S", 44},
                    {"4H", 43},
                    {"4D", 42},
                    {"4C", 41},
                    {"4S", 40},
                    {"5H", 39},
                    {"5D", 38},
                    {"5C", 37},
                    {"5S", 36},
                    {"6H", 35},
                    {"6D", 34},
                    {"6C", 33},
                    {"6S", 32},
                    {"7H", 31},
                    {"7D", 30},
                    {"7C", 29},
                    {"7S", 28},
                    {"8H", 27},
                    {"8D", 26},
                    {"8C", 25},
                    {"8S", 24},
                    {"9H", 23},
                    {"9D", 22},
                    {"9C", 21},
                    {"9S", 20},
                    {"TH", 19},
                    {"TD", 18},
                    {"TC", 17},
                    {"TS", 16},
                    {"JH", 15},
                    {"JD", 14},
                    {"JC", 13},
                    {"JS", 12},
                    {"QH", 11},
                    {"QD", 10},
                    {"QC", 9},
                    {"QS", 8},
                    {"KH", 7},
                    {"KD", 6},
                    {"KC", 5},
                    {"KS", 4},
                    {"AH", 3},
                    {"AD", 2},
                    {"AC", 1},
                    {"AS", 0},
                };

        private static readonly int[] AceLowStraightCardRanks =
            new[] {'a', '2', '3', '4', '5'}.Select(card => RankToScore[card]).ToArray();

        private static readonly int[] AceLowStraight = AceLowStraightCardRanks.OrderBy(rank => rank).ToArray();
        private static readonly MatchResult NoMatch = new MatchResult(Int32.MaxValue, false);

        private static readonly IHandClassifier[] HandClassifiersInOrder =
            new IHandClassifier[]
                {
                    new FiveOfAKindClassifier(),
                    new StraightFlushClassifier(),
                    new FourOfAKindClassifier(),
                    new FullHouseClassifier(),
                    new FlushClassifier(),
                    new StraightClassifier(),
                    new ThreeOfAKindClassifier(),
                    new TwoPairClassifier(),
                    new PairClassifier(),
                    new HighCardClassifier(),
                };

        static SimpleRanker()
        {
            foreach (var kvp in CardToScore)
            {
                ScoreToCard[kvp.Value] = kvp.Key;
                CardRank[kvp.Value] = RankToScore[kvp.Key[0]];
                CardSuit[kvp.Value] = SuitToScore[kvp.Key[1]];
            }
        }

        #region IRanker Members

        public int ScoreCard(string card)
        {
            Debug.Assert(!String.IsNullOrEmpty(card));
            Debug.Assert(card.Length == 2);
            Debug.Assert(CardToScore.ContainsKey(card));
            return CardToScore[card];
        }

        public int ScoreHand(IEnumerable<int> cards)
        {
            Debug.Assert(cards != null);
            var cardsAsArray = cards.ToArray();
            Debug.Assert(cardsAsArray.Length == 5);

            foreach (var handClassifier in HandClassifiersInOrder)
            {
                var matchResult = handClassifier.Match(cardsAsArray);
                if (matchResult.Matched)
                    return matchResult.Score;
            }
            return int.MaxValue;
        }

        #endregion

        private static bool IsHighStraight(IEnumerable<int> ranks)
        {
            var ranksInOrderAsArray = ranks.OrderBy(rank => rank).ToArray();
            return
                (ranksInOrderAsArray[0] == (ranksInOrderAsArray[1] - 1) &&
                 ranksInOrderAsArray[0] == (ranksInOrderAsArray[2] - 2) &&
                 ranksInOrderAsArray[0] == (ranksInOrderAsArray[3] - 3) &&
                 ranksInOrderAsArray[0] == (ranksInOrderAsArray[4] - 4))
                ;
        }

        private static bool IsLowStraight(IEnumerable<int> ranks)
        {
            return ranks.OrderBy(rank => rank).SequenceEqual(AceLowStraight);
        }

        private static bool IsFlush(IEnumerable<int> suits)
        {
            return suits.GroupBy(suit => suit).Count() == 1;
        }

        internal static int ScoreRanks(IEnumerable<int> ranks, HandOrder handOrder)
        {
            var ranksAsArray = ranks.ToArray();
            return
                ((int) handOrder) << 20
                | ranksAsArray[0] << 16
                | ranksAsArray[1] << 12
                | ranksAsArray[2] << 8
                | ranksAsArray[3] << 4
                | ranksAsArray[4];
        }

        #region Nested type: FiveOfAKindClassifier

        private sealed class FiveOfAKindClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                if (groupCounts.Length == 1)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.FiveOfAKind));

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: FlushClassifier

        private sealed class FlushClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                // cards need to be ordered to allow for comparing flushes.
                var cardsAsArray = cards.ToArray();
                var suits = cardsAsArray.Select(card => CardSuit[card]);
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderBy(rank => rank);

                if (IsFlush(suits))
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.Flush));

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: FourOfAKindClassifier

        private sealed class FourOfAKindClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                if (groupCounts.Length != 2) return NoMatch;

                if (groupCounts[0] == 4 && groupCounts[1] == 1)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.FourOfAKind));

                if (groupCounts[0] == 1 && groupCounts[1] == 4)
                {
                    // reorder hand so the four of a kind is in front of the kicker
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[0],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.FourOfAKind));
                }

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: FullHouseClassifier

        private sealed class FullHouseClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(group => group.Count()).ToArray();

                if (groupCounts.Length != 2) return NoMatch;

                if (groupCounts[0] == 3 && groupCounts[1] == 2)
                {
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.FullHouse));
                }

                if (groupCounts[0] == 2 && groupCounts[1] == 3)
                {
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[1],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.FullHouse));
                }

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: HandOrder

        internal enum HandOrder
        {
            FiveOfAKind = 0,
            StraightFlush = 1,
            FourOfAKind = 2,
            FullHouse = 3,
            Flush = 4,
            Straight = 5,
            ThreeOfAKind = 6,
            TwoPair = 7,
            OnePair = 8,
            HighCard = 9
        }

        #endregion

        #region Nested type: HighCardClassifier

        private sealed class HighCardClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.HighCard));
            }

            #endregion
        }

        #endregion

        #region Nested type: IHandClassifier

        private interface IHandClassifier
        {
            IMatchResult Match(IEnumerable<int> cards);
        }

        #endregion

        #region Nested type: IMatchResult

        private interface IMatchResult
        {
            int Score { get; }
            bool Matched { get; }
        }

        #endregion

        #region Nested type: MatchResult

        private sealed class MatchResult : IMatchResult
        {
            public MatchResult(int score, bool matched = true)
            {
                Score = score;
                Matched = matched;
            }

            #region IMatchResult Members

            public int Score { get; private set; }
            public bool Matched { get; private set; }

            #endregion
        }

        #endregion

        #region Nested type: PairClassifier

        private sealed class PairClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                if (groupCounts.Length != 4) return NoMatch;

                if (groupCounts[0] == 2)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.OnePair));

                if (groupCounts[1] == 2)
                {
                    // reorder hand so the pair in front of the kickers
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.OnePair));
                }

                if (groupCounts[2] == 2)
                {
                    // reorder hand so the pair in front of the kickers
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[4],
                            };

                    return new MatchResult(ScoreRanks(reordered, HandOrder.OnePair));
                }

                if (groupCounts[3] == 2)
                {
                    // reorder hand so the pair in front of the kickers
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[2],
                            };

                    return new MatchResult(ScoreRanks(reordered, HandOrder.OnePair));
                }

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: StraightClassifier

        private sealed class StraightClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var cardsAsArray = cards.ToArray();
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();

                if (IsLowStraight(ranksInOrderAsArray))
                    return new MatchResult(ScoreRanks(AceLowStraightCardRanks, HandOrder.Straight));

                if (IsHighStraight(ranksInOrderAsArray))
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.Straight));

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: StraightFlushClassifier

        private sealed class StraightFlushClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var cardsAsArray = cards.ToArray();
                var suits = cardsAsArray.Select(card => CardSuit[card]);
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();

                if (IsFlush(suits))
                {
                    if (IsHighStraight(ranksInOrderAsArray))
                        return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.StraightFlush));

                    if (IsLowStraight(ranksInOrderAsArray))
                        return new MatchResult(ScoreRanks(AceLowStraightCardRanks, HandOrder.StraightFlush));
                }
                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: ThreeOfAKindClassifier

        private sealed class ThreeOfAKindClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                if (groupCounts.Length != 3) return NoMatch;

                if (groupCounts[0] == 3)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.ThreeOfAKind));

                if (groupCounts[1] == 3)
                {
                    // reorder hand so the three of a kind is in front of the kicker
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[4],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.ThreeOfAKind));
                }

                if (groupCounts[2] == 3)
                {
                    // reorder hand so the three of a kind is in front of the kicker
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[1],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.ThreeOfAKind));
                }

                return NoMatch;
            }

            #endregion
        }

        #endregion

        #region Nested type: TwoPairClassifier

        private sealed class TwoPairClassifier : IHandClassifier
        {
            #region IHandClassifier Members

            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderBy(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                // we know it has to be (1 2 2), (2 1 2), or (2 2 1) because the three of a kind classifier
                // would have matched on (1 1 3), (1 3 1), or (3 1 1).

                if (groupCounts.Length != 3) return NoMatch;

                if (groupCounts[0] == 1)
                {
                    // reorder hand so the two of a kind is in front of the kicker
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[2],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[0],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.TwoPair));
                }

                if (groupCounts[1] == 1)
                {
                    // reorder hand so the two of a kind is in front of the kicker
                    var reordered =
                        new[]
                            {
                                ranksInOrderAsArray[0],
                                ranksInOrderAsArray[1],
                                ranksInOrderAsArray[3],
                                ranksInOrderAsArray[4],
                                ranksInOrderAsArray[2],
                            };
                    return new MatchResult(ScoreRanks(reordered, HandOrder.TwoPair));
                }

                if (groupCounts[2] == 1)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.TwoPair));

                return NoMatch;
            }

            #endregion
        }

        #endregion
    }
}