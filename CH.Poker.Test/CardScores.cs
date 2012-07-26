using System.Collections.Generic;
using NUnit.Framework;

namespace CH.Poker.Test
{
    [TestFixture]
    public sealed class CardScores
    {
        [Test]
        public void CardScoresAreValid()
        {
            foreach (var ranker in _rankers)
            {
                // ensure card map to unique values
                var scores = new HashSet<int>();
                foreach (var suit in new[] {'H', 'D', 'C', 'S'})
                    foreach (var rank in new[] {'2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'})
                    {
                        var card = new string(new[] {rank, suit});
                        var score = ranker.ScoreCard(card);
                        Assert.That(score, Is.GreaterThanOrEqualTo(0));
                        Assert.That(score, Is.LessThanOrEqualTo(51));
                        Assert.That(scores, Is.Not.Member(scores));
                        scores.Add(score);
                    }
            }
        }

        private readonly IRanker[] _rankers = new IRanker[] {new SimpleRanker()};//, new TwoPlusTwoRanker()};
    }
}