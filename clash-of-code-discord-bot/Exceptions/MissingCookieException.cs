namespace clash_of_code_bot.Exceptions;

public class MissingCookieException : Exception
{
    public MissingCookieException(string? message) : base(message)
    {
    }
}