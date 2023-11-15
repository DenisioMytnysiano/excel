using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Leaf;

public class DoubleLeafNode: LeafNode
{
    private readonly double _value;

    public DoubleLeafNode(double value)
    {
        _value = value;
    }
    public override CellResult Evaluate(IEvaluationContext context)
    {
        return new DoubleResult(_value);
    }
}