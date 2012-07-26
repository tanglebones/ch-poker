using System;
using System.Collections.Generic;

namespace CH.Poker.App.Test
{
    internal sealed class TestCase : ITestCase
    {
        private readonly Func<IEnumerable<string>, string> _checker;
        private readonly IEnumerable<string> _expectedOutputLines;
        private readonly IEnumerable<string> _inputLines;
        private readonly string _testName;

        public TestCase(string testName, IEnumerable<string> inputLines, IEnumerable<string> expectedOutputLines)
        {
            _testName = testName;
            _inputLines = inputLines;
            _expectedOutputLines = expectedOutputLines;
            _checker = DefaultChecker;
        }

        public TestCase(string testName, IEnumerable<string> inputLines, Func<IEnumerable<string>, string> checker)
        {
            _testName = testName;
            _inputLines = inputLines;
            _checker = checker;
        }

        public IEnumerable<string> ExpectedOutputLines
        {
            get { return _expectedOutputLines; }
        }


        public string TestName
        {
            get { return _testName; }
        }

        public IEnumerable<string> InputLines
        {
            get { return _inputLines; }
        }

        public Func<IEnumerable<string>, string> Checker
        {
            get { return _checker; }
        }


        private string DefaultChecker(IEnumerable<string> outputLines)
        {
            var outputLineCursor = outputLines.GetEnumerator();

            foreach (var expectedLine in ExpectedOutputLines)
            {
                if (!outputLineCursor.MoveNext()) return "More lines expected.";
                var outputLine = outputLineCursor.Current;
                if (outputLine != expectedLine)
                    return "Output not as expected: " + Environment.NewLine + "Expected:\t" + expectedLine +
                           Environment.NewLine + "\t" + "Actual:\t" + outputLine;
            }
            while (outputLineCursor.MoveNext())
                if (!string.IsNullOrEmpty(outputLineCursor.Current))
                    return "More lines returned than expected";
            return null;
        }

        public override string ToString()
        {
            return TestName;
        }
    }
}