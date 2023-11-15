using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Tree;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using Excel.Core.FormulaEngine.Lexer;

namespace Excel.Core.FormulaEngine;

public interface IFormulaEngine
{
    public CellResult Evaluate(IEvaluationContext context, Cell cell);
}

public class FormulaEngine: IFormulaEngine
{
    private readonly ILexer _lexer;

    public FormulaEngine(ILexer lexer)
    {
        _lexer = lexer;
    }
    public CellResult Evaluate(IEvaluationContext context, Cell cell)
    {
        if (cell.Value is not FormulaValue formula)
        {
            return cell.Result;
        }
        return EvaluateFormula(context, formula);
    }

    private CellResult EvaluateFormula(IEvaluationContext context, FormulaValue formula)
    {
        try
        {
            var tokens = _lexer.Parse(formula.Expression).ToList();
            var tree = AbstractSyntaxTree.Builder().FromTokens(tokens);
            return tree.Evaluate(context);
        }
        catch (Exception ex)
        {
            return new ErrorResult();
        }
    }
}