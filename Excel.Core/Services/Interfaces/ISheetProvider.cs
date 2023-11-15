using Excel.Core.Exceptions;

namespace Excel.Core.Services.Interfaces;

public interface ISheetProvider
{
    public ISheet GetSheet(SheetId identifier);
}

public class SheetId
{
    public string Id { get; private set; }

    public static SheetId Create(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new SheetIdIsNullOrEmptyException();
        }
        return new SheetId(id.ToLower());
    }

    private SheetId(string id)
    {
        Id = id;
    }
}