using System;
using CH.Poker.App.Impl;

namespace CH.Poker.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var inputTextReader = Console.In;
            var outputTextWriter = Console.Out;
            var errorTextWriter = Console.Error;

            Implementation.Run(inputTextReader, outputTextWriter, errorTextWriter);
        }
    }
}