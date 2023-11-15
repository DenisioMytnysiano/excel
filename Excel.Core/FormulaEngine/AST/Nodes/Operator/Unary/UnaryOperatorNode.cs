namespace Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;

public abstract class UnaryOperatorNode: OperatorNode
{
    protected Node Node { get; }

    protected UnaryOperatorNode(Node node)
    {
        Node = node;
    }
}