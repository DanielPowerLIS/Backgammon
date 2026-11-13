using System;

namespace Backgammon.Server.SupportServices.Exceptions
{
    public class AccountCodeUnvailableException : Exception
    {
        public AccountCodeUnvailableException(string message) : base(message)
        {
        }
    }
}
