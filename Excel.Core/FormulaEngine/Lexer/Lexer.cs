using System.Text;
using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Nodes.Operator.Unary;
using Excel.Core.FormulaEngine.Token;
using static System.String;

namespace Excel.Core.FormulaEngine.Lexer;

public class Lexer: ILexer
{
    private const string TokenDelimiter = "~";
    
    private readonly IDictionary<string, Token.Token> _tokenOperatorMapping = new Dictionary<string, Token.Token>()
    {
        { "+", PlusToken.Instance },
        { "-", MinusToken.Instance },
        { "*", MultiplyToken.Instance },
        { "/", DivideToken.Instance },
        { "(", LeftBraceToken.Instance },
        { ")", RightBraceToken.Instance },
        { "'", QuoteToken.Instance },
        {"max", MaxFunctionToken.Instance},
        {"min", MinFunctionToken.Instance},
        {"sum", SumFunctionToken.Instance},
        {"avg", AverageFunctionToken.Instance},
        {"external_ref", ExternalCellToken.Instance},
        {",", CommaToken.Instance}
    };
    
    public IEnumerable<Token.Token> Parse(string formula)
    {
        if (IsNullOrEmpty(formula))
        {
            return Enumerable.Empty<Token.Token>();
        }

        return ProcessTokens(GetTokens(formula.ToLower())
            .Split(TokenDelimiter)
            .Select(ResolveToken)
            .ToList());
    }

    private string GetTokens(string formula)
    {
        var result = new StringBuilder();
        var operators = _tokenOperatorMapping.Keys.ToHashSet();
        var isInString = false;
        for (int i = 0; i < formula.Length; i++)
        {
            var currentElement = formula[i];
            
            if (currentElement == ' ')
            {
                continue;
            }

            if (currentElement is '\"' or '\'')
            {
                isInString = !isInString;
            }
            
            var previousElement = i - 1 >= 0 ? formula[i - 1] : char.MaxValue;
            if (operators.Contains(currentElement.ToString()) && (!isInString || currentElement is '\'' or '\"'))
            {
                var leftDelimiter = i == 0 || operators.Contains(previousElement.ToString()) ? "" : TokenDelimiter;
                var rightDelimiter = i + 1 == formula.Length ? "" : TokenDelimiter;
                result.Append($"{leftDelimiter}{currentElement}{rightDelimiter}");
            }
            else
            {
                result.Append(currentElement);
            }
        }
        return result.ToString();
    }

    private Token.Token ResolveToken(string token)
    {
        if (_tokenOperatorMapping.TryGetValue(token, out var value))
        {
           return value;
        }

        if (double.TryParse(token, out var parsedDouble))
        {
            return new DoubleToken(parsedDouble);
        }

        return new CellToken(CellId.Create(token));
    }
    
    private IEnumerable<Token.Token> ProcessTokens(IList<Token.Token> tokens)
    {
        var result = new List<Token.Token>();
        
        for (int i = 0; i < tokens.Count; i++)
        {
            var currentToken = tokens[i];
            var previousToken = i - 1 >= 0 ? tokens[i - 1] : null;
            var nextToken = i + 1 < tokens.Count ? tokens[i + 1] : null;
            var token = ReEvaluateTokenBasedOnContext(currentToken, previousToken, nextToken);
            if (token != null)
            {
                result.Add(token);
            }
        }
        
        return result;
    }

    private Token.Token? ReEvaluateTokenBasedOnContext(Token.Token token, Token.Token? previousToken, Token.Token? nextToken)
    {
        if (token is MinusToken && !IsOrOperandToken(previousToken))
        {
            return NegateToken.Instance;
        }

        if (token is CellToken cell && previousToken is QuoteToken && nextToken is QuoteToken)
        {
            return new StringToken(cell.CellReference.Id);
        }
        
        if (token is QuoteToken)
        {
            return null;
        }
        
        return token;
    }

    private bool IsOrOperandToken(Token.Token? token)
    {
        return token is CellToken or DoubleToken or RightBraceToken;
    }
}