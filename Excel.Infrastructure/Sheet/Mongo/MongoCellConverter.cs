using Excel.Core.Entities;
using Excel.Core.FormulaEngine.Lexer;
using Excel.Core.FormulaEngine.Token;
using Excel.Core.Services.Interfaces;

namespace Excel.Infrastructure.Sheet.Mongo;

public class MongoCellConverter
{
    private readonly ILexer _lexer;

    public MongoCellConverter(ILexer lexer)
    {
        _lexer = lexer;
    }
    public Cell ToCell(MongoCell cell, ISheet sheet)
    {
        return new Cell(CellId.Create(cell.CellId), CellValue.Parse(cell.Value), CellResult.Parse(cell.Result), sheet);
    }

    public MongoCell FromCell(Cell cell, ISheet sheet)
    {
        var dependencies = new List<string>();
        
        if (cell.Value is FormulaValue fv)
        {
           dependencies.AddRange(_lexer.Parse(fv.Expression).OfType<CellToken>().Select(x => x.CellReference.Id));
        }

        return new MongoCell
        {
            CellId = cell.Id.Id,
            SheetId = sheet.GetSheetId().Id,
            Result = cell.Result.ToString(),
            Value = cell.Value.ToString(),
            DependsOn = dependencies
        };
    }
}