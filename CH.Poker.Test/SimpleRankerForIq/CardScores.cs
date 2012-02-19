using System.Collections.Generic;
using NUnit.Framework;

namespace CH.Poker.Test.SimpleRankerForIq
{
    [TestFixture]
    public sealed class CardScores
    {
        private readonly ISet<int> _scores = new HashSet<int>();
        private readonly Poker.SimpleRankerForIq _ranker = new Poker.SimpleRankerForIq();

        [SetUp]
        public void SetUp()
        {
            // Also ensures card map to unique values
            foreach (var suit in new[] { 'H', 'D', 'C', 'S' })
                foreach (var rank in new[] { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' })
                {
                    var card = new string(new[] { rank, suit });
                    var score = _ranker.ScoreCard(card);
                    Assert.That(_scores, Is.Not.Member(_scores));
                    _scores.Add(score);
                }
        }

        [Test]
        public void AreInDocumentedRange()
        {
            Assert.That(_scores.Count, Is.GreaterThan(0));
            foreach (var score in _scores)
            {
                Assert.That(score, Is.GreaterThanOrEqualTo(0));
                Assert.That(score, Is.LessThanOrEqualTo(51));
            }
        }
    }
}