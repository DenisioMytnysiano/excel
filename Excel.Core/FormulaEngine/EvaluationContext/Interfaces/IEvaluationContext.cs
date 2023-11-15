using Excel.Core.Entities;

namespace Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

public interface IEvaluationContext
{
    public CellResult? GetExternalCellValue(string url);
    public CellResult? GetCellValue(CellId identifier);
    public void UpdateContext(Cell cell);
}