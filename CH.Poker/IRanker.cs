using System.Collections.Generic;

namespace CH.Poker
{
    /// <summary>
    ///   This ranking iterface was chosen based on my previous knowledge of high speed poker ranking algoritms and may seem a little weird at first. We map each card into a 0 to 51 number space to make lookups in internal arrays easier when we start to move into higher speed algorithms. We also don't report back hand order (family, e.g. Flush) or any information about the cards for breaking ties. Each hand is boiled down to a single number to make sorted hands relative to each other something that can be down easily and effiently in the calling code.
    /// </summary>
    public interface IRanker
    {
        /// <summary>
        ///   Maps a card represented by a string to a number from 0 to 51. The exact mapping can vary from implementation to implementation.
        /// </summary>
        /// <param name="card"> Valid ranks are 2 through 9, T, J, Q, K, A and are case sensitive. </param>
        /// <returns> A number from 0 to 51. </returns>
        int ScoreCard(string card);

        /// <summary>
        ///   Maps an array of at least five cards (some implementations may support up to seven cards, creating the base five card hand from the seven) and returns a numberic score for the hand that can be used to compare hands. Lower scores are better.
        /// </summary>
        /// <param name="ranks"> The cards in the hand, as scored by ScoreCard </param>
        /// <returns> A score for the hand, lower being better, that can be used to compare the hard with other hands. </returns>
        int ScoreHand(IEnumerable<int> ranks);
    }
}