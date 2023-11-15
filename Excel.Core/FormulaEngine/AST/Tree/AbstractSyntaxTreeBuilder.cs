using Excel.Core.FormulaEngine.AST.Nodes;
using Excel.Core.FormulaEngine.AST.Nodes.Leaf;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Binary;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Nary;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;
using Excel.Core.FormulaEngine.Token;

namespace Excel.Core.FormulaEngine.AST.Tree;

public class AbstractSyntaxTreeBuilder
{
    public AbstractSyntaxTree FromTokens(IEnumerable<Token.Token> tokens)
    {
        var rpnTokens = ToReversePolishNotation(tokens);
        return BuildFromReversePolishNotation(rpnTokens);
    }
    
    private IEnumerable<Token.Token> ToReversePolishNotation(IEnumerable<Token.Token> tokens)
    {
        var reversedTokens = new List<Token.Token>();
        var stack = new Stack<Token.Token>();
        foreach (var token in tokens)
        {
            if (token.IsOperand())
            {
                reversedTokens.Add(token);
            }
            
            else if (token is LeftBraceToken or NegateToken)
            {
                stack.Push(token);
            }
            
            else if(token is RightBraceToken)
            {
                while (stack.Count > 0 && stack.Peek() is not LeftBraceToken)
                {
                    reversedTokens.Add(stack.Pop());
                }
                stack.Pop();
            }
            else
            {
                while (stack.Count > 0 
                       && TokenOperatorPriority.GetPriority(token) <= TokenOperatorPriority.GetPriority(stack.Peek()) 
                       && TokenOperatorAssociativity.GetPriority(token) == TokenAssociativity.Left)
                {
                    reversedTokens.Add(stack.Pop());
                }
                stack.Push(token);
            }
            
        }


        while (stack.Count > 0)
        {
            reversedTokens.Add(stack.Pop());
        }

        return reversedTokens;
    }

    private AbstractSyntaxTree BuildFromReversePolishNotation(IEnumerable<Token.Token> tokens)
    {
        var stack = new Stack<Node>();

        foreach (var token in tokens)
        {
            if (token.IsOperand())
            {
                stack.Push(ResolveLeafNode(token));
            }
            else if(token.IsBinaryOperator())
            {
                var firstOperator = stack.Pop();
                var secondOperator = stack.Pop();
                stack.Push(ResolveBinaryOperatorNode(token, secondOperator, firstOperator));
            }
            else if(token.IsUnaryOperator())
            {
                var value = stack.Pop();
                stack.Push(ResolveUnaryOperatorNode(token, value));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        return new AbstractSyntaxTree(stack.Pop());
    }

    private Node ResolveLeafNode(Token.Token token)
    {
        return token switch
        {
            DoubleToken d => new DoubleLeafNode(d.Value),
            StringToken s => new StringLeafNode(s.Value),
            CellToken c => new CellReferenceLeafNode(c.CellReference),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }

    private Node ResolveBinaryOperatorNode(Token.Token token, Node firstOperand, Node secondOperand)
    {
        return token switch
        {
            PlusToken => new PlusOperatorNode(firstOperand, secondOperand),
            MinusToken => new MinusOperatorNode(firstOperand, secondOperand),
            MultiplyToken => new MultiplyOperatorNode(firstOperand, secondOperand),
            DivideToken => new DivideOperatorNode(firstOperand, secondOperand),
            CommaToken => new CommaOperatorNode(firstOperand, secondOperand),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }

    private Node ResolveUnaryOperatorNode(Token.Token token, Node operand)
    {
        return token switch
        {
            NegateToken => new NegateOperatorNode(operand),
            SumFunctionToken => new SumOperatorNode(operand),
            AverageFunctionToken => new AverageOperatorNode(operand),
            MinFunctionToken => new MinOperatorNode(operand),
            MaxFunctionToken => new MaxOperatorNode(operand),
            ExternalCellToken => new ExternalRefOperatorNode(operand), 
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }
}