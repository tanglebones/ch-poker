using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CH.Poker.App.Impl;
using NUnit.Framework;

namespace CH.Poker.App.Test
{
    [TestFixture]
    public static class Driver
    {
        private const string ProgramUnderTest = @"../../../CH.Poker.App/bin/Debug/CH.Poker.App.exe";
        private const int ProgramUnderTestTimeoutBeforeTestFails = 60000;

        public static IEnumerable<ITestCase> Cases
        {
            get
            {
                var interfaceToMatch = typeof (ITestCaseProvider);
                return
                    Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => !t.IsInterface && interfaceToMatch.IsAssignableFrom(t))
                        .Select(type => (ITestCaseProvider) Activator.CreateInstance(type))
                        .SelectMany(instance => instance.Cases);
            }
        }

        [Test]
        [TestCaseSource(typeof (Driver), "Cases")]
        public static void ExecuteTestCasesViaConsoleApp(ITestCase testCase)
        {
            var p = Process.Start(
                new ProcessStartInfo
                    {
                        FileName = ProgramUnderTest,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                );

            Assert.That(p, Is.Not.Null);

            foreach (var line in testCase.InputLines)
                p.StandardInput.WriteLine(line);

            p.StandardInput.Close();

            Assert.That(p.WaitForExit(ProgramUnderTestTimeoutBeforeTestFails));
            var errors = p.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errors))
            {
                Debug.WriteLine(errors);
                Assert.Fail("Program failed with an error.");
            }
            var outputLines = new List<string>();

            for (;;)
            {
                var outputLine = p.StandardOutput.ReadLine();
                if (outputLine == null) break;
                outputLines.Add(outputLine);
            }

            p.Dispose();

            var result = testCase.Checker(outputLines);
            if (result == null) return;

            Debug.WriteLine("Output: ");
            Debug.WriteLine(string.Join(Environment.NewLine, outputLines));
            Assert.Fail(result);
        }

        [Test]
        [TestCaseSource(typeof (Driver), "Cases")]
        public static void ExecuteTestCasesViaInternalRun(ITestCase testCase)
        {
            var inputTextReader = new StringReader(string.Join(Environment.NewLine, testCase.InputLines));
            var outputTextStringBuilder = new StringBuilder();
            var outputTextWriter = new StringWriter(outputTextStringBuilder);
            var errorTextStringBuilder = new StringBuilder();
            var errorTextWriter = new StringWriter(errorTextStringBuilder);
            try
            {
                Implementation.Run(inputTextReader, outputTextWriter, errorTextWriter);

                var errors = errorTextStringBuilder.ToString();
                if (!string.IsNullOrEmpty(errors))
                {
                    Debug.WriteLine(errors);
                    Assert.Fail("Program failed with an error.");
                }
                var outputLines = outputTextStringBuilder.ToString().Split(new[] {Environment.NewLine},
                                                                           StringSplitOptions.None);

                var result = testCase.Checker(outputLines);
                if (result == null) return;

                Debug.WriteLine("Output: ");
                Debug.WriteLine(string.Join(Environment.NewLine, outputLines));
                Assert.Fail(result);
            }
            finally
            {
                outputTextWriter.Dispose();
                errorTextWriter.Dispose();
                inputTextReader.Dispose();
            }
        }
    }
}