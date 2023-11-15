using Excel.Core.Entities;
using Excel.Core.FormulaEngine.AST.Tree;
using Excel.Core.FormulaEngine.Lexer;
using Excel.Core.FormulaEngine.Token;
using Xunit;

namespace Excel.Tests;

public class LexerTests
{
    private readonly ILexer _lexer = new Lexer();

    [Theory, MemberData(nameof(GetFormulasAndTokens))]
    public void Parse_Returns_ExpectedTokens(string formula, IEnumerable<Token> expectedTokens)
    {
        var tokens = _lexer.Parse(formula).ToList();
        Assert.True(tokens.SequenceEqual(expectedTokens));
    }

    public static IEnumerable<object[]> GetFormulasAndTokens()
    {
        return new List<object[]>
        {
            new object[] {"", new List<Token>()},
            new object[] {"-abc", new List<Token> {NegateToken.Instance, new CellToken(CellId.Create("abc"))}},
            new object[] {"abc+'abc'", new List<Token> {new CellToken(CellId.Create("abc")), PlusToken.Instance, new StringToken("abc")}},
            new object[] {"-5", new List<Token> {NegateToken.Instance, new DoubleToken(5.0)}},
            new object[] {"-25/5", new List<Token> {NegateToken.Instance, new DoubleToken(25.0), DivideToken.Instance, new DoubleToken(5.0)}},
            new object[] {"2-5", new List<Token> {new DoubleToken(2.0), MinusToken.Instance, new DoubleToken(5.0)}},
            new object[] {"2+5", new List<Token> {new DoubleToken(2.0), PlusToken.Instance, new DoubleToken(5.0)}},
            new object[] {"abc+5", new List<Token> {new CellToken(CellId.Create("abc")), PlusToken.Instance, new DoubleToken(5.0)}},
            new object[] {"abc*def", new List<Token> {new CellToken(CellId.Create("abc")), MultiplyToken.Instance, new CellToken(CellId.Create("def"))}},
            new object[] {"2/def", new List<Token> {new DoubleToken(2.0), DivideToken.Instance, new CellToken(CellId.Create("def"))}},
            new object[] {"(2-abc)*def", new List<Token> {LeftBraceToken.Instance, new DoubleToken(2.0), MinusToken.Instance, new CellToken(CellId.Create("abc")), RightBraceToken.Instance, MultiplyToken.Instance, new CellToken(CellId.Create("def"))}},
            new object[] {"3+5/(4-2)", new List<Token> {new DoubleToken(3.0), PlusToken.Instance, new DoubleToken(5.0), DivideToken.Instance, LeftBraceToken.Instance, new DoubleToken(4.0), MinusToken.Instance, new DoubleToken(2.0), RightBraceToken.Instance}},
            new object[] {"external_ref('a/b')", new List<Token>{ExternalCellToken.Instance, LeftBraceToken.Instance, new StringToken("a/b"), RightBraceToken.Instance}}
        };
    }
}