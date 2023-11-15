using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Leaf;

public class StringLeafNode: LeafNode
{
    private readonly string _value;

    public StringLeafNode(string value)
    {
        _value = value;
    }
    public override CellResult Evaluate(IEvaluationContext context)
    {
        return new StringResult(_value);
    }
}