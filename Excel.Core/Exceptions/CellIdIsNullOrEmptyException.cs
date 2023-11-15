namespace Excel.Core.Exceptions;

public class CellIdIsNullOrEmptyException: CoreException
{
    private const string ErrorMessage = "Cell identifier cannot be null or empty string";
    
    public CellIdIsNullOrEmptyException() : base(ErrorMessage)
    {
    }
}