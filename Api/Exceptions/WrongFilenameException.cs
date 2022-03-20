using System;

namespace Api.Exceptions
{
    public class WrongFilenameException : Exception
    {
        public WrongFilenameException(string message)
            : base(message) { }
    }
}