using Excel.Core.Entities;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;

public abstract class BinaryOperatorNode: OperatorNode
{
    protected Node First { get; }
    protected Node Second { get; }

    protected BinaryOperatorNode(Node first, Node second)
    {
        First = first;
        Second = second;
    }
}