using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CH.Poker.Test.SimpleRanker
{
    [TestFixture]
    public sealed class HandScores
    {
        private IEnumerable<int> _fiveOfAKind;
        private IEnumerable<int> _straightFlush;
        private IEnumerable<int> _quads;
        private IEnumerable<int> _fullhouse;
        private IEnumerable<int> _flush;
        private IEnumerable<int> _straight;
        private IEnumerable<int> _trips;
        private IEnumerable<int> _twoPair;
        private IEnumerable<int> _pair;
        private IEnumerable<int> _highcard;
        private IRanker _ranker;

        [SetUp]
        public void SetUp()
        {
            _ranker = new Poker.SimpleRanker();
            _fiveOfAKind = new[] { "2H", "2D", "2C", "2S", "2H" }.Select(_ranker.ScoreCard);
            _straightFlush = new[] { "2H", "3H", "5H", "6H", "4H" }.Select(_ranker.ScoreCard);
            _quads = new[] { "2H", "2D", "2C", "2S", "4H" }.Select(_ranker.ScoreCard);
            _fullhouse = new[] { "8H", "8D", "8C", "AS", "AH" }.Select(_ranker.ScoreCard);
            _flush = new[] { "2H", "3H", "5H", "6H", "7H" }.Select(_ranker.ScoreCard);
            _straight = new[] { "2H", "3H", "5H", "6H", "4C" }.Select(_ranker.ScoreCard);
            _trips = new[] { "2H", "2D", "2C", "6H", "7H" }.Select(_ranker.ScoreCard);
            _twoPair = new[] { "2H", "2D", "3C", "3H", "7H" }.Select(_ranker.ScoreCard);
            _pair = new[] { "2H", "2D", "3C", "6H", "7H" }.Select(_ranker.ScoreCard);
            _highcard = new[] {"2H", "3D", "9C", "6H", "7H"}.Select(_ranker.ScoreCard);
        }

        [Test]
        public void HandsOrderComparisonsAreCorrect()
        {
            var scores =
                new[]
                    {
                        _ranker.ScoreHand(_fiveOfAKind),
                        _ranker.ScoreHand(_straightFlush),
                        _ranker.ScoreHand(_quads),
                        _ranker.ScoreHand(_fullhouse),
                        _ranker.ScoreHand(_flush),
                        _ranker.ScoreHand(_straight),
                        _ranker.ScoreHand(_trips),
                        _ranker.ScoreHand(_twoPair),
                        _ranker.ScoreHand(_pair),
                        _ranker.ScoreHand(_highcard)
                    };
            var set = new HashSet<int>();
            foreach(var score in scores) set.Add(score);
            Assert.That(set.Count, Is.EqualTo(scores.Length)); // unique scores
            var sorted = scores.OrderBy(score => score);
            Assert.That(sorted.SequenceEqual(scores));
        }

        [Test]
        public void CardOrderDoesNotChangeResultingScore()
        {
            foreach (var hand in new[] {_fiveOfAKind, _straightFlush, _quads, _fullhouse, _flush, _straight, _trips, _twoPair, _pair, _highcard})
            {
                var concHand = hand.ToArray();
                var score = _ranker.ScoreHand(concHand);
                var permutations = Permute(concHand).ToArray();

                // test would silently fail if permute returned an empty set, so make sure it doesn't.
                Assert.That(permutations.Length, Is.GreaterThan(0));

                foreach (var permutation in permutations)
                    Assert.That(_ranker.ScoreHand(permutation), Is.EqualTo(score));
            }
        }

        private void HandsAreInOrder(params IEnumerable<string>[] hands)
        {
            HandsAreInOrder(hands.AsEnumerable());
        }

        private void HandsAreInOrder(IEnumerable<IEnumerable<string>> hands)
        {
            var convertedHands = hands.Select(hand => hand.Select(_ranker.ScoreCard));

            var scored = convertedHands.Select(_ranker.ScoreHand).ToArray();
            var sorted = scored.OrderBy(score => score).ToArray();
            Assert.That(scored.SequenceEqual(sorted));
        }

        private void HandsAreEqual(params IEnumerable<string>[] hands)
        {
            HandsAreEqual(hands.AsEnumerable());
        }

        private void HandsAreEqual(IEnumerable<IEnumerable<string>> hands)
        {
            var convertedHands = hands.Select(hand => _ranker.ScoreHand(hand.Select(_ranker.ScoreCard))).ToArray();
            Assert.That(convertedHands.Length,Is.GreaterThan(1));
            var first = convertedHands[0];
            for(var i=1;i<convertedHands.Length;++i)
                Assert.That(convertedHands[i],Is.EqualTo(first));
        }


        [Test]
        public void ComparingFiveOfAKindsGoesToTheHigherFiveOfAKind()
        {
            HandsAreInOrder(
                new[] { "AH", "AD", "AC", "AS", "AH" },
                new[] { "KH", "KD", "KC", "KS", "KH" },
                new[] { "QH", "QD", "QC", "QS", "QH" },
                new[] { "JH", "JD", "JC", "JS", "JH" },
                new[] { "TH", "TD", "TC", "TS", "TH" },
                new[] { "9H", "9D", "9C", "9S", "9H" },
                new[] { "8H", "8D", "8C", "8S", "8H" },
                new[] { "7H", "7D", "7C", "7S", "7H" },
                new[] { "6H", "6D", "6C", "6S", "6H" },
                new[] { "5H", "5D", "5C", "5S", "5H" },
                new[] { "4H", "4D", "4C", "4S", "4H" },
                new[] { "3H", "3D", "3C", "3S", "3H" },
                new[] { "2H", "2D", "2C", "2S", "2H" }
                );
        }

        [Test]
        public void ComparingStraightFlushesGoesToTheHigherStraightFlush()
        {
            HandsAreInOrder(
                new[] { "AS", "KS", "QS", "JS", "TS" },
                new[] { "KS", "QS", "JS", "TS", "9S" },
                new[] { "QS", "JS", "TS", "9S", "8S" },
                new[] { "JS", "TS", "9S", "8S", "7S" },
                new[] { "TS", "9S", "8S", "7S", "6S" },
                new[] { "9S", "8S", "7S", "6S", "5S" },
                new[] { "8S", "7S", "6S", "5S", "4S" },
                new[] { "7S", "6S", "5S", "4S", "3S" },
                new[] { "6S", "5S", "4S", "3S", "2S" },
                new[] { "5S", "4S", "3S", "2S", "AS" }
            );
        }

        [Test]
        public void DifferentSuitedStraightFlushesAreEqual()
        {
            var ranks = new[] {'A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2'};
            var suits = new[] {'H', 'C', 'D', 'S'};

            for(var rankSkip = 0; rankSkip<ranks.Length-5;++rankSkip)
            {
                var skip = rankSkip; // avoid access to modified closure warning
                var hands = suits.Select(suit => ranks.Skip(skip).Take(5).Select(rank => new string(new[] {rank, suit})));
                HandsAreEqual(hands);
            }
        }

        [Test]
        public void ComparingQuadsGoesToTheHigherQuads()
        {
            HandsAreInOrder(
                new[] { "AH", "AD", "AC", "AS", "2H" },
                new[] { "KH", "KD", "KC", "KS", "3H" },
                new[] { "QH", "QD", "QC", "QS", "4H" },
                new[] { "JH", "JD", "JC", "JS", "5H" },
                new[] { "TH", "TD", "TC", "TS", "6H" },
                new[] { "9H", "9D", "9C", "9S", "7H" },
                new[] { "8H", "8D", "8C", "8S", "7H" },
                new[] { "7H", "7D", "7C", "7S", "9H" },
                new[] { "6H", "6D", "6C", "6S", "TH" },
                new[] { "5H", "5D", "5C", "5S", "JH" },
                new[] { "4H", "4D", "4C", "4S", "QH" },
                new[] { "3H", "3D", "3C", "3S", "KH" },
                new[] { "2H", "2D", "2C", "2S", "AH" }
            );
        }

        [Test]
        public void ComparingFullHouseGoesToTheHigherFullHouse()
        {
            HandsAreInOrder(
                new[] { "AH", "AD", "AS", "3H", "3D" },
                new[] { "KH", "KD", "KS", "2H", "2D" },
                new[] { "QH", "QD", "QS", "4H", "4D" },
                new[] { "JH", "JD", "JS", "5H", "5D" },
                new[] { "TH", "TD", "TS", "AH", "AD" },
                new[] { "9H", "9D", "9S", "AH", "AD" },
                new[] { "8H", "8D", "8S", "9H", "9D" },
                new[] { "7H", "7D", "7S", "9H", "9D" },
                new[] { "6H", "6D", "6S", "TH", "TD" },
                new[] { "5H", "5D", "5S", "JH", "JD" },
                new[] { "4H", "4D", "4S", "QH", "QD" },
                new[] { "3H", "3D", "3S", "KH", "KD" },
                new[] { "2H", "2D", "2S", "AH", "AD" }
                );
        }

        [Test]
        public void ComparingFlushesGoesToTheHigherFlush()
        {
            HandsAreInOrder(
                new[] {"AH", "KH", "QH", "JH", "2H"},
                new[] {"AH", "KH", "QH", "3H", "2H"},
                new[] {"AH", "KH", "4H", "3H", "2H"},
                new[] {"AH", "6H", "4H", "3H", "2H"},
                new[] {"7H", "5H", "4H", "3H", "2H"}
                );
        }

        [Test]
        public void ComparingStraightGoesToTheHigherStraight()
        {
            HandsAreInOrder(
                new[] { "AS", "KS", "QS", "JS", "TC" },
                new[] { "KS", "QS", "JS", "TS", "9C" },
                new[] { "QS", "JS", "TS", "9S", "8C" },
                new[] { "JS", "TS", "9S", "8S", "7C" },
                new[] { "TS", "9S", "8S", "7S", "6C" },
                new[] { "9S", "8S", "7S", "6S", "5C" },
                new[] { "8S", "7S", "6S", "5S", "4C" },
                new[] { "7S", "6S", "5S", "4S", "3C" },
                new[] { "6S", "5S", "4S", "3S", "2C" },
                new[] { "5S", "4S", "3S", "2S", "AC" }
            );
        }

        [Test]
        public void ComparingTripsGoesToTheHigherTrip()
        {
            HandsAreInOrder(
                new[] {"AH", "AD", "AS", "3H", "2H"},
                new[] {"KH", "KD", "KS", "3H", "2H"},
                new[] {"QH", "QD", "QS", "4H", "2H"},
                new[] {"JH", "JD", "JS", "5H", "2H"},
                new[] {"TH", "TD", "TS", "6H", "2H"},
                new[] {"9H", "9D", "9S", "7H", "2H"},
                new[] {"8H", "8D", "8S", "9H", "2H"},
                new[] {"7H", "7D", "7S", "9H", "2H"},
                new[] {"6H", "6D", "6S", "TH", "2H"},
                new[] {"5H", "5D", "5S", "JH", "2H"},
                new[] {"4H", "4D", "4S", "QH", "2H"},
                new[] {"3H", "3D", "3S", "KH", "2H"},
                new[] {"2H", "2D", "2S", "AH", "3H"}
                );
        }

        [Test]
        public void SortOrderOfTwoPairCasesDoesNotChangeScore()
        {
            HandsAreInOrder(
                new[] {"8H", "8D", "9S", "3D", "3H"},
                new[] {"8H", "8D", "6S", "3D", "3H"},
                new[] {"8H", "8D", "2S", "3D", "3H"}
                );
        }

        [Test]
        public void ComparingTwoPairsGoesToTheHigherTwoPair()
        {
            HandsAreInOrder(
                new[] { "AH", "AD", "2S", "2H", "3H" },
                new[] { "KH", "KD", "3S", "3H", "2H" },
                new[] { "QH", "QD", "4S", "4H", "2H" },
                new[] { "JH", "JD", "5S", "5H", "2H" },
                new[] { "TH", "TD", "6S", "6H", "2H" },
                new[] { "9H", "9D", "7S", "7H", "2H" },
                new[] { "8H", "8D", "9S", "2D", "2H" },
                new[] { "7H", "7D", "9S", "2D", "2H" },
                new[] { "6H", "6D", "TS", "2D", "2H" },
                new[] { "5H", "5D", "JS", "2D", "2H" },
                new[] { "4H", "4D", "QS", "2D", "2H" },
                new[] { "3H", "3D", "KS", "2D", "2H" }
                );
        }

        [Test]
        public void ComparingPairsGoesToTheHigherPair()
        {
            HandsAreInOrder(
                new[] {"AH", "AD", "2S", "JH", "3H"},
                new[] {"KH", "KD", "3S", "JH", "2H"},
                new[] {"QH", "QD", "4S", "JH", "2H"},
                new[] {"JH", "JD", "5S", "9H", "2H"},
                new[] {"TH", "TD", "6S", "JH", "2H"},
                new[] {"9H", "9D", "7S", "JH", "2H"},
                new[] {"8H", "8D", "9S", "JH", "2H"},
                new[] {"7H", "7D", "9S", "JH", "2H"},
                new[] {"6H", "6D", "TS", "JH", "2H"},
                new[] {"5H", "5D", "JS", "TH", "2H"},
                new[] {"4H", "4D", "QS", "JH", "2H"},
                new[] {"3H", "3D", "KS", "JH", "2H"},
                new[] {"2H", "2D", "AS", "JH", "3H"}
                );
        }

        [Test]
        public void ComparingHighcardGoesToTheHigherCard()
        {
            HandsAreInOrder(
                new[] { "AH", "KD", "QS", "3H", "2H" },
                new[] { "KH", "QD", "JS", "3H", "2H" },
                new[] { "QH", "JD", "TS", "4H", "2H" },
                new[] { "JH", "TD", "9S", "5H", "2H" },
                new[] { "TH", "9D", "8S", "6H", "2H" },
                new[] { "9H", "8D", "7S", "5H", "2H" },
                new[] { "8H", "7D", "6S", "3H", "2H" },
                new[] { "7H", "6D", "5S", "4H", "2H" }
                );
        }


        // produces every possible ordering of the elements in a enumerable.
        // should consider creating a separate library and publishing a nuget package for this...
        //  ... and we could make it an extention method.
        private static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> enumerable)
        {
            var concEnumerable = enumerable.ToArray();

            if (concEnumerable.Length == 1)
            {
                yield return concEnumerable;
            }
            else
                for (var i = 0; i < concEnumerable.Length; ++i)
                {
                    // remove i from first and make it last in the result, append the permutations of the remaining elements
                    var newFirst = concEnumerable[i];
                    var localCopyOfIndex = i; // used to avoid closure on a loop variable.
                    var newSubHand = concEnumerable.Where((t, j) => j != localCopyOfIndex);
                    foreach (var subPermute in Permute(newSubHand))
                        yield return new[] {newFirst}.Concat(subPermute);
                }
        }

        [Test]
        public void PermuteIsCorrectForFourElements()
        {
            var set = new HashSet<string>();
            foreach (var permutation in Permute(new[] {'a', 'b', 'c', 'd'}))
            {
                set.Add(new string(permutation.ToArray()));
            }
            Assert.That(set.Count, Is.EqualTo(24));
        }
    }
}