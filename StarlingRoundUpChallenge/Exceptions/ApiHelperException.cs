namespace StarlingRoundUpChallenge.Exceptions;

public class ApiHelperException : Exception
{
    public ApiHelperException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
}