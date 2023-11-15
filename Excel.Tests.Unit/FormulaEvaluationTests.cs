using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Tree;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using Excel.Core.FormulaEngine.Lexer;
using NSubstitute;
using Xunit;

namespace Excel.Tests;

public class FormulaEvaluationTests
{
    private readonly ILexer _lexer = new Lexer();

    [Theory, MemberData(nameof(GetFormulasAndResults))]
    public void Evaluate_Returns_EvaluatedFormulaValue(string formula, CellResult evaluatedResult)
    {
        var tokens = _lexer.Parse(formula).ToList();
        var evaluationContext = GetEvaluationContext();
        var tree = AbstractSyntaxTree.Builder().FromTokens(tokens);
        var value = tree.Evaluate(evaluationContext);
        Assert.Equal(evaluatedResult, value);
    }

    public static IEnumerable<object[]> GetFormulasAndResults()
    {
        return new List<object[]>
        {
            new object[] {"-abc", new DoubleResult(-5)},
            new object[] {"abc+'abc'", new StringResult("5abc")},
            new object[] {"-5", new DoubleResult(-5)},
            new object[] {"-25/5", new DoubleResult(-5)},
            new object[] {"2-5", new DoubleResult(-3)},
            new object[] {"2+5", new DoubleResult(7)},
            new object[] {"abc+5", new DoubleResult(10)},
            new object[] {"abc*def", new DoubleResult(50)},
            new object[] {"2/def", new DoubleResult(0.2)},
            new object[] {"(2-abc)*def", new DoubleResult(-30)},
            new object[] {"(2-abc)*'def'", new ErrorResult()},
            new object[] {"3+5/-(4-2)", new DoubleResult(0.5)},
            new object[]{"max(abc, max(2, 3))", new DoubleResult(5)},
            new object[]{"min(abc, def, 7, max(2, 3))", new DoubleResult(3)},
            new object[]{"sum(abc, def, 7)", new DoubleResult(22)},
            new object[]{"avg(max(abc, 3), def - 1 + 1 * 5, min(7, 10))", new DoubleResult(26/3.0)},
            new object[]{"min(-5, 2)", new DoubleResult(-5)}
        };
    }

    private IEvaluationContext GetEvaluationContext()
    {
        var context = Substitute.For<IEvaluationContext>();
        context.GetCellValue(CellId.Create("abc")).Returns(new DoubleResult(5));
        context.GetCellValue(CellId.Create("def")).Returns(new DoubleResult(10));
        return context;
    }
}