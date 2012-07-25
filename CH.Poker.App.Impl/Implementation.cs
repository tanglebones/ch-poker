using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CH.Poker.App.Impl
{
    public static class Implementation
    {
        private static readonly IRanker Ranker = new SimpleRanker();
        private static readonly char[] ValidRanks;
        private static readonly char[] ValidSuits;

        static Implementation()
        {
            ValidRanks =
                new[]
                    {
                        '2',
                        '3',
                        '4',
                        '5',
                        '6',
                        '7',
                        '8',
                        '9',
                        'T',
                        'J',
                        'Q',
                        'K',
                        'A',
                    };

            ValidSuits =
                new[]
                    {
                        'H',
                        'D',
                        'S',
                        'C',
                    };
        }

        internal static void Run(TextReader inputTextReader, TextWriter outputTextWriter, TextWriter errorTextWriter)
        {
            try
            {
                var hands = ParseHands(inputTextReader).ToArray();

                if (hands.Length == 0)
                    throw new InvalidInputException("Error: no input.");

                var winningHand = ScoreHands(hands).OrderBy(hand => hand.Score).First();

                outputTextWriter.WriteLine(winningHand.Owner);
            }
            catch (InvalidInputException invalidInputException)
            {
                outputTextWriter.WriteLine(invalidInputException.Message);
            }
            catch (Exception exception)
            {
                errorTextWriter.WriteLine(exception.Message);
            }
        }

        private static IEnumerable<IHand> ScoreHands(IEnumerable<IHand> hands)
        {
            foreach (var hand in hands)
            {
                hand.Score = RankHand(hand);
                yield return hand;
            }
        }

        private static int RankHand(IHand hand)
        {
            return Ranker.ScoreHand(hand.Cards.Select(card => Ranker.ScoreCard(card)));
        }

        private static IEnumerable<IHand> ParseHands(TextReader inputTextReader)
        {
            var ownersSeen = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            for (var lineNumber = 0;; ++lineNumber)
            {
                var line = inputTextReader.ReadLine();
                if (line == null) break;
                if (String.IsNullOrWhiteSpace(line)) continue;

                var fields = ParseFields(lineNumber, line);
                var owner = fields.First();
                int seenPreviouslyOnLine;
                if (ownersSeen.TryGetValue(owner, out seenPreviouslyOnLine))
                {
                    throw new InvalidInputException("Error: Owner \"" + owner + "\" on line " + lineNumber +
                                                    " was already specified on line " + seenPreviouslyOnLine);
                }
                ownersSeen[owner] = lineNumber;
                var cards = ParseCards(lineNumber, fields);

                yield return
                    new Hand(
                        owner,
                        cards
                        );
            }
        }

        private static string[] ParseFields(int lineNumber, string line)
        {
            var fields = line.Split(',').Select(field => field.Trim()).ToArray();

            if (fields.Length != 6)
                throw new InvalidInputException(
                    "Error: Invalid input on line " + lineNumber +
                    ", expected exactly 6 fields [owner, card, card, card, card, card], got:" +
                    Environment.NewLine + "\t" + line
                    );
            return fields;
        }

        private static IEnumerable<string> ParseCards(int lineNumber, IEnumerable<string> fields)
        {
            var seenPreviously = new HashSet<string>();
            var cards = fields.Skip(1).Select(card => card.Replace("10", "T").ToUpperInvariant()).ToArray();

            var cardErrors = new List<string>();
            foreach (var card in cards)
            {
                if (seenPreviously.Contains(card))
                    cardErrors.Add("duplicate card \"" + card + "\" in hard.");
                else
                    seenPreviously.Add(card);

                char rank;
                char suit;
                switch (card.Length)
                {
                    case 2:
                        rank = card[0];
                        suit = card[1];
                        break;
                    default:
                        cardErrors.Add("card \"" + card + "\" is not a valid.");
                        continue;
                }

                if (!ValidRanks.Contains(rank))
                    cardErrors.Add("card \"" + card + "\" does not have a valid rank.");

                if (!ValidSuits.Contains(suit))
                    cardErrors.Add("card \"" + card + "\" does not have a valid suit.");
            }
            var cardErrorsAsArray = cardErrors.ToArray();
            if (cardErrorsAsArray.Any())
                throw new InvalidInputException(
                    "Error: Invalid input on line " + lineNumber + ", " +
                    String.Join(Environment.NewLine + "\t", cardErrors)
                    );
            return cards;
        }
    }
}