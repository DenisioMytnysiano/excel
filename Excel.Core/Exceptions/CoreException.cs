namespace Excel.Core.Exceptions;

public class CoreException: Exception
{
    public CoreException(string message, Exception inner = null) : base(message, inner)
    {
        
    }
}