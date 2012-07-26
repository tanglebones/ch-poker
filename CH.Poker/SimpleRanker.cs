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

        private static readonly int[] AceLowStraightCardRanks =
            new[] {'a', '2', '3', '4', '5'}.Select(card => RankToScore[card]).ToArray();

        private static readonly int[] AceLowStraight = AceLowStraightCardRanks.OrderBy(rank => rank).ToArray();
        private static readonly MatchResult NoMatch = new MatchResult(Int32.MaxValue, false);

        private static readonly IHandClassifier[] HandClassifiersInReverseOrder =
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

            foreach (var handClassifier in HandClassifiersInReverseOrder)
            {
                var matchResult = handClassifier.Match(cardsAsArray);
                if (matchResult.Matched)
                    return matchResult.Score;
            }
            return -1;
        }


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

        private static int ScoreRanks(IEnumerable<int> ranks, HandOrder handOrder)
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


        private sealed class FiveOfAKindClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
                var groupCounts = ranksInOrderAsArray.GroupBy(rank => rank).Select(g => g.Count()).ToArray();

                if (groupCounts.Length == 1)
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.FiveOfAKind));

                return NoMatch;
            }
        }


        private sealed class FlushClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                // cards need to be ordered to allow for comparing flushes.
                var cardsAsArray = cards.ToArray();
                var suits = cardsAsArray.Select(card => CardSuit[card]);
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderByDescending(rank => rank);

                if (IsFlush(suits))
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.Flush));

                return NoMatch;
            }
        }


        private sealed class FourOfAKindClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
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
        }


        private sealed class FullHouseClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
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
        }


        private enum HandOrder
        {
            FiveOfAKind = 9,
            StraightFlush = 8,
            FourOfAKind = 7,
            FullHouse = 6,
            Flush = 5,
            Straight = 4,
            ThreeOfAKind = 3,
            TwoPair = 2,
            OnePair = 1,
            HighCard = 0
        }

        private sealed class HighCardClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
                return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.HighCard));
            }
        }


        private interface IHandClassifier
        {
            IMatchResult Match(IEnumerable<int> cards);
        }


        private interface IMatchResult
        {
            int Score { get; }
            bool Matched { get; }
        }

        private sealed class MatchResult : IMatchResult
        {
            public MatchResult(int score, bool matched = true)
            {
                Score = score;
                Matched = matched;
            }

            public int Score { get; private set; }
            public bool Matched { get; private set; }
        }

        private sealed class PairClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
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
        }


        private sealed class StraightClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var cardsAsArray = cards.ToArray();
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();

                if (IsLowStraight(ranksInOrderAsArray))
                    return new MatchResult(ScoreRanks(AceLowStraightCardRanks, HandOrder.Straight));

                if (IsHighStraight(ranksInOrderAsArray))
                    return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.Straight));

                return NoMatch;
            }
        }


        private sealed class StraightFlushClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var cardsAsArray = cards.ToArray();
                var suits = cardsAsArray.Select(card => CardSuit[card]);
                var ranksInOrderAsArray = cardsAsArray.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();

                if (IsFlush(suits))
                {
                    if (IsHighStraight(ranksInOrderAsArray))
                        return new MatchResult(ScoreRanks(ranksInOrderAsArray, HandOrder.StraightFlush));

                    if (IsLowStraight(ranksInOrderAsArray))
                        return new MatchResult(ScoreRanks(AceLowStraightCardRanks, HandOrder.StraightFlush));
                }
                return NoMatch;
            }
        }


        private sealed class ThreeOfAKindClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
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
        }


        private sealed class TwoPairClassifier : IHandClassifier
        {
            public IMatchResult Match(IEnumerable<int> cards)
            {
                var ranksInOrderAsArray = cards.Select(card => CardRank[card]).OrderByDescending(rank => rank).ToArray();
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
        }
    }
}