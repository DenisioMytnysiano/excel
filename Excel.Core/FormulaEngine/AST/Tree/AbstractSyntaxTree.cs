using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;

namespace Excel.Core.FormulaEngine.AST.Tree;

public class AbstractSyntaxTree
{
    private readonly Node _rootNode;

    public AbstractSyntaxTree(Node rootNode)
    {
        _rootNode = rootNode;
    }

    public CellResult Evaluate(IEvaluationContext context)
    {
        return _rootNode.Evaluate(context);
    }

    public static AbstractSyntaxTreeBuilder Builder()
    {
        return new AbstractSyntaxTreeBuilder();
    }
}