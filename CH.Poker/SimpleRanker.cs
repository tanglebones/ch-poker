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
                    {'a', 13}, // low ace
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

        enum HandOrder
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

        private static readonly HandOrder[] MatchesToHandOrder =
            new[]
                {
                    HandOrder.HighCard, // 0000 = HC
                    HandOrder.OnePair, // 0001 = 1P
                    HandOrder.OnePair, // 0010 = 1P
                    HandOrder.ThreeOfAKind, // 0011 = 3K
                    HandOrder.OnePair, // 0100 = 1P
                    HandOrder.TwoPair, // 0101 = 2P
                    HandOrder.ThreeOfAKind, // 0110 = 3K
                    HandOrder.FourOfAKind, // 0111 = 4K
                    HandOrder.OnePair, // 1000 = 1P
                    HandOrder.TwoPair, // 1001 = 2P
                    HandOrder.TwoPair, // 1010 = 2P
                    HandOrder.FullHouse, // 1011 = FH
                    HandOrder.ThreeOfAKind, // 1100 = 3K
                    HandOrder.FullHouse, // 1101 = FH
                    HandOrder.FourOfAKind, // 1110 = 4K
                    HandOrder.FiveOfAKind, // 1111 = 5K
                };

        // We need to move pairs, trips, and quads to the front of the hand so they
        // compare before the unmatched cards when hands are of the same order.
        private static readonly Func<int[], int[]>[] MatchesToReorderFunction =
            new Func<int[], int[]>[]
                {
                    x=>x, // 0000
                    x=>x, // 0001
                    x=>new[]{x[1],x[2],x[0],x[3],x[4]}, // 0010
                    x=>x, // 0011
                    x=>new[]{x[2],x[3],x[0],x[1],x[4]}, // 0100
                    x=> // 0101
                    CardRank[x[0]]<CardRank[x[2]]
                        ? x
                        : new[]{x[2],x[3],x[0],x[1],x[4]},
                    x=>new[]{x[1],x[2],x[3],x[0],x[4]}, // 0110
                    x=>x, // 0111
                    x=>new[]{x[3],x[4],x[0],x[1],x[2]}, // 1000
                    x=> // 1001
                    CardRank[x[0]]<CardRank[x[3]]
                        ? x
                        : new[]{x[3],x[4],x[0],x[1],x[2]},
                    x=> // 1010
                    CardRank[x[1]]<CardRank[x[3]]
                        ? x
                        : new[]{x[3],x[4],x[1],x[2],x[0]},
                    x=>x, // 1011
                    x=>new[]{x[2],x[3],x[4],x[0],x[1]}, // 1100
                    x=>new[]{x[2],x[3],x[4],x[0],x[1]}, // 1101
                    x=>new[]{x[1],x[2],x[3],x[4],x[0]}, // 1110
                    x=>x, // 1111
                };

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

        private static readonly int[] AceLowStraight = new[] {0,9,10,11,12};
        private static readonly int[] AceLowStraightCardRanks = new [] {'a','2','3','4','5'}.Select(card=>RankToScore[card]).ToArray();

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

            // Use a simple direct approach for now. [ Comparivity simple ;) ]

            // sort cards
            var cardsAsArray = cards.OrderBy(card => CardRank[card]).ToArray();
            var cardRanksAsArray = cardsAsArray.Select(card => CardRank[card]).ToArray();
            Debug.Assert(cardsAsArray.Length == 5);

            // check for flush
            var firstSuit = CardSuit[cardsAsArray[0]];
            var isFlush = cardsAsArray.Skip(1).Aggregate(true, (current, card) => current && CardSuit[card] == firstSuit);
            var isAceLowStraight = cardRanksAsArray.SequenceEqual(AceLowStraight);
            var isStraight = isAceLowStraight ||
                (cardRanksAsArray[0] == (cardRanksAsArray[1] - 1) &&
                 cardRanksAsArray[0] == (cardRanksAsArray[2] - 2) &&
                 cardRanksAsArray[0] == (cardRanksAsArray[3] - 3) &&
                 cardRanksAsArray[0] == (cardRanksAsArray[4] - 4))
                ;

            HandOrder handOrder;
            if (!(isFlush || isStraight))
            {
                // if it is not a flush compare each neighbor
                // this will produce four comparisons, with 16 possible results
                // we use tables to map the result unto a hand order and move the grouped
                // cards to the start of the hand.
                var matches = 0;
                var prevCardRank = cardRanksAsArray[0];
                var bit = 1;
                foreach (var cardRank in cardRanksAsArray.Skip(1))
                {
                    if (prevCardRank == cardRank)
                        matches |= bit;
                    bit *= 2;
                    prevCardRank = cardRank;
                }
                handOrder = MatchesToHandOrder[matches];
                cardRanksAsArray = MatchesToReorderFunction[matches](cardRanksAsArray);
            }
            else
            {
                if (isAceLowStraight)
                    cardRanksAsArray = AceLowStraightCardRanks;


                if (isStraight && isFlush)
                {
                    handOrder = HandOrder.StraightFlush;
                }
                else if(isFlush)
                {
                    handOrder = HandOrder.Flush;
                }
                else
                {
                    handOrder = HandOrder.Straight;
                }
            }

            // We map the hand onto a single number that can be compared by placing the
            // hand order in the most significate bits, and the each card rank following
            // on in order of importance to the comparison.
            return
                ((int)handOrder) << 20
                | cardRanksAsArray[0] << 16
                | cardRanksAsArray[1] << 12
                | cardRanksAsArray[2] << 8
                | cardRanksAsArray[3] << 4
                | cardRanksAsArray[4];
        }
    }
}