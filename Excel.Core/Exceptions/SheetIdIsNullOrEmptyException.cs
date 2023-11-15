namespace Excel.Core.Exceptions;

public class SheetIdIsNullOrEmptyException: CoreException
{
    private const string ErrorMessage = "Sheet identifier cannot be null or empty string";
    
    public SheetIdIsNullOrEmptyException() : base(ErrorMessage)
    {
    }
}