using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Leaf;

public class CellReferenceLeafNode: LeafNode
{
    private readonly CellId _cellReference;

    public CellReferenceLeafNode(CellId cellReference)
    {
        _cellReference = cellReference;
    }
    public override CellResult Evaluate(IEvaluationContext context)
    {
        var resolvedValue = context.GetCellValue(_cellReference);
        return resolvedValue ?? new ErrorResult();
    }
}