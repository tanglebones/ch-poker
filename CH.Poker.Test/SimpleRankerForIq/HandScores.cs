using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CH.Poker.Test.SimpleRankerForIq
{
    [TestFixture]
    public sealed class HandScores
    {
        private IEnumerable<int> _flush;
        private IEnumerable<int> _trips;
        private IEnumerable<int> _pair;
        private IEnumerable<int> _highcard;
        private IRanker _ranker;

        [SetUp]
        public void SetUp()
        {
            _ranker = new Poker.SimpleRankerForIq();
            _flush = new[] {"2H", "3H", "5H", "6H", "7H"}.Select(_ranker.ScoreCard);
            _trips = new[] {"2H", "2D", "2C", "6H", "7H"}.Select(_ranker.ScoreCard);
            _pair = new[] {"2H", "2D", "3C", "6H", "7H"}.Select(_ranker.ScoreCard);
            _highcard = new[] {"2H", "3D", "9C", "6H", "7H"}.Select(_ranker.ScoreCard);
        }

        [Test]
        public void HandsOrderComparisonsAreCorrect()
        {
            var flushScore = _ranker.ScoreHand(_flush);
            var tripsScore = _ranker.ScoreHand(_trips);
            var pairScore = _ranker.ScoreHand(_pair);
            var highcardScore = _ranker.ScoreHand(_highcard);

            Assert.That(flushScore, Is.LessThan(tripsScore));
            Assert.That(flushScore, Is.LessThan(pairScore));
            Assert.That(flushScore, Is.LessThan(highcardScore));

            Assert.That(tripsScore, Is.LessThan(pairScore));
            Assert.That(tripsScore, Is.LessThan(highcardScore));

            Assert.That(pairScore, Is.LessThan(highcardScore));
        }

        [Test]
        public void CardOrderDoesNotChangeResultingScore()
        {
            foreach (var hand in new[] {_flush, _trips, _pair, _highcard})
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
            var convertedHands = hands.Select(hand => hand.Select(_ranker.ScoreCard));

            var scored = convertedHands.Select(_ranker.ScoreHand).ToArray();
            var sorted = scored.OrderBy(score => score).ToArray();
            Assert.That(scored.SequenceEqual(sorted));
        }

        [Test]
        public void ComparingFlushesGoesToTheHigherFlush()
        {
            HandsAreInOrder(
                new[] {"AH", "KH", "QH", "JH", "2H"},
                new[] {"AH", "KH", "QH", "3H", "2H"},
                new[] {"AH", "KH", "4H", "3H", "2H"},
                new[] {"AH", "5H", "4H", "3H", "2H"},
                new[] {"7H", "5H", "4H", "3H", "2H"}
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
                new[] {"8H", "8D", "8S", "8H", "2H"},
                new[] {"7H", "7D", "7S", "9H", "2H"},
                new[] {"6H", "6D", "6S", "TH", "2H"},
                new[] {"5H", "5D", "5S", "JH", "2H"},
                new[] {"4H", "4D", "4S", "QH", "2H"},
                new[] {"3H", "3D", "3S", "KH", "2H"},
                new[] {"2H", "2D", "2S", "AH", "2H"}
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