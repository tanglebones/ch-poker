using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal class TestsForRankingAcrossHandTypes : ITestCaseProvider
    {
        public IEnumerable<ITestCase> Cases
        {
            get
            {
                yield return
                    new TestCase(
                        "Flush is better than 3 of a kind",
                        new[]
                            {
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                                "Bob, 3C, 3D, 3S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );
                yield return
                    new TestCase(
                        "Flush is better than 3 of a kind, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 3C, 3D, 3S, 8C, 10D",
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "Flush is better than 1 Pair",
                        new[]
                            {
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                                "Bob, 9C, 3D, 3S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "Flush is better than 1 Pair, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 9C, 3D, 3S, 8C, 10D",
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "Flush is better than high card",
                        new[]
                            {
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "Flush is better than high card, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                                "Joe, 3H, 4H, 5H, 6H, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "3 of a kind is better than 1 Pair",
                        new[]
                            {
                                "Joe, 3H, 4H, 4C, 4D, 8H",
                                "Bob, 9C, 3D, 3S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "3 of a kind is better than 1 Pair, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 9C, 3D, 3S, 8C, 10D",
                                "Joe, 3H, 4H, 4C, 4D, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "3 of a kind is better than high card",
                        new[]
                            {
                                "Joe, 3H, 4H, 4C, 4D, 8H",
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "3 of a kind is better than high card, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                                "Joe, 3H, 4H, 4C, 4D, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );
                yield return
                    new TestCase(
                        "1 pair is better than high card",
                        new[]
                            {
                                "Joe, 3H, 4H, 5C, 4D, 8H",
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );

                yield return
                    new TestCase(
                        "1 pair is better than high card, even when it is the second hand given.",
                        new[]
                            {
                                "Bob, 9C, 3D, 2S, 8C, 10D",
                                "Joe, 3H, 4H, 5C, 4D, 8H",
                            },
                        new[]
                            {
                                "Joe"
                            }
                        );
            }
        }
    }
}