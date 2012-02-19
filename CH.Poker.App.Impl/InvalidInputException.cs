using System;

namespace CH.Poker.App.Impl
{
    [Serializable]
    internal sealed class InvalidInputException : Exception
    {
        public InvalidInputException(string message)
            : base(message)
        {
        }
    }
}