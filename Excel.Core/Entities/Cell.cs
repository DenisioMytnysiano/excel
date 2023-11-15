using Excel.Core.Exceptions;
using Excel.Core.Services.Interfaces;

namespace Excel.Core.Entities;

public class CellId
{
    public string Id { get; }

    public static CellId Create(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new CellIdIsNullOrEmptyException();
        }
        return new CellId(id.ToLower());
    }

    private bool Equals(CellId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((CellId) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    private CellId(string id)
    {
        Id = id;
    }
}

public record Cell(CellId Id, CellValue Value, CellResult Result, ISheet Sheet)
{
    public Cell ToError()
    {
        return this with {Result = new ErrorResult()};
    }
}