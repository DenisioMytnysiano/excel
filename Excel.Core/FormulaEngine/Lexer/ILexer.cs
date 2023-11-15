namespace Excel.Core.FormulaEngine.Lexer;

public interface ILexer
{
    public IEnumerable<Token.Token> Parse(string formula);
}