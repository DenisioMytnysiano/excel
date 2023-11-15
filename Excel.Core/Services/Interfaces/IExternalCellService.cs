using Excel.Core.Entities;

namespace Excel.Core.Services.Interfaces;

public interface IExternalCellService
{
    public CellResult? GetExternalCellResult(Uri uri);
}