using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Nary;
using Excel.Core.FormulaEngine.EvaluationContext.Interfaces;
using NSubstitute;
using Xunit;

namespace Excel.Tests;

public class MinOperatorTests
{
    [Fact]
    public void Evaluate_Returns_MinimumOfValues_When_EvaluatedValuesAreDoubles()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();
        
        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new CellRangeResult(new List<CellResult>
        {
            new DoubleResult(10),
            new DoubleResult(15),
        }));

        var averageNode = new MinOperatorNode(childNode);
        var result = averageNode.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(10, ((DoubleResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_Value_When_EvaluatedThereIsSingleValue()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();
        
        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new CellRangeResult(new List<CellResult>
        {
            new DoubleResult(10),
        }));

        var averageNode = new MinOperatorNode(childNode);
        var result = averageNode.Evaluate(evaluationContext);
        
        Assert.True(result is DoubleResult);
        Assert.Equal(10, ((DoubleResult)result).Value);
    }
    
    [Fact]
    public void Evaluate_Returns_ErrorResult_When_AnyOperandIsNotDouble()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();
        
        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new CellRangeResult(new List<CellResult>
        {
            new DoubleResult(10),
            new StringResult("ddsds")
        }));

        var averageNode = new MinOperatorNode(childNode);
        var result = averageNode.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
    
    [Fact]
    public void Evaluate_Returns_ErrorResult_When_ThereAreNoOperands()
    {
        var evaluationContext = Substitute.For<IEvaluationContext>();
        
        var childNode = Substitute.For<Node>();
        childNode.Evaluate(evaluationContext).Returns(new CellRangeResult(new List<CellResult>()));

        var averageNode = new MinOperatorNode(childNode);
        var result = averageNode.Evaluate(evaluationContext);
        
        Assert.True(result is ErrorResult);
    }
}