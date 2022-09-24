namespace clash_of_code_bot.Exceptions;

public class MissingTokenException : Exception
{
    public MissingTokenException(string? message) : base(message)
    {
    }
}