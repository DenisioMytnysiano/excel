## Tecnhological Stack
In this project I used .NET to implement the backend part of the application. API is build on FastEnpoints framework which uses REPR (Request-Enpoint-Response) pattern which enforces better code organization and readablilty. As a storage engine I chose Mongodb because it is highly scalable database that can also be effectively used as a graph database (the reasons for that are discovered in Data Format section).

## Domain Modelling
The main entity introduced is Cell, according to requirements it can have formula or primitive type value and primitive or error type result. That is why the following type hierarchy is used to model a cell entity.
```
CellValue
-> StringValue
-> DoubleValue
-> FormulaValue

CellResult
-> StringResult
-> DoubleResult
-> ErrorResult
-> RangeResult

CellId
SheetId
```
As a result Cell consists of identifier, value, result and sheet reference. CellId and SheetId classes encapsulate the logic that this identifiers have to be case-insensitive<br>

Another abstraction added is sheet. It serves as a store for cells and is aware of their dependencies. The ISheet interface is the following:
```c#
public interface ISheet
{
    public SheetId GetSheetId();
    
    public Task<IEnumerable<Cell>> GetCells(IEnumerable<CellId> ids);
    
    public Task<IEnumerable<Cell>> GetCells();
    
    public Task<IEnumerable<Cell>> GetCellsDependencies(IEnumerable<Cell> identifier);
    
    public Task<IEnumerable<Cell>> GetAffectedCells(Cell cell);
    
    public Task UpdateCells(IEnumerable<Cell> cells);
    
    public Task Delete();
}
```

## Formula Engine Implementation
Formula engine is using AST to evaluate expressions. The process of building the AST tree incudes:
- parsing formula into the tokens with lexer
- converting tokens to reverse polish notation
- build AST tree from reverse polish notation

### Parsing Formula Into Tokens With Lexer
In this projects tokens are represented as classes. Token hierarchy is following:
```
Token
-> SingletonToken
  -> PlusToken
  -> MinusToken
  -> DivideToken
  -> MultiplyToken
  -> LeftBraceToken
  -> RightBraceToken
  -> QuoteToken
  -> ExternalCellToken
  -> SumFunctionToken
  -> MaxFunctionToken
  -> MinFunctionToken
  -> AvgfunctionToken
-> DoubleToken
-> CellToken
-> StringToken
```
Singleton token was added to not allocate extra memory on operation tokens that will always be the same. Quote token was added to distinguish cell reference and string in formulas. 
Lexer is responsible for converting formula to tokens to the interface is following:
```c#
public interface ILexer
{
    public IEnumerable<Token.Token> Parse(string formula);
}
```

### Converting Tokens To Reverse Polish Notation
To covert token expression to reverse polish notation shunting yard algorithm is used.

### Building AST tree
AST Tree contains of compositionally linked nodes. Two types of nodes are present - operator and operand nodes. operator nodes are evaluated based on the underlying nodes that are treated as opearnds. Node type hierarchy is the following
```
Node
-> OperatorNode
   -> BinaryOperatorNode
      -> PlusOperatorNode
      -> MinusOperatorNode
      -> DivideOperatorNode
      -> MultiplyOperatorNode
   -> UnaryOperatorNode
      -> NegateOperator
   -> RangeOperatorNode
      -> SumOperatorNode
      -> MaxOperatorNode
      -> MinOperatorNode
      -> AverageOperatorNode
-> LeafNode
   -> StringLeafNode
   -> DoubleLeafNode
   -> CellReferenceLeafNode 
```
Node value can be evaluated so I defined the following interface:
```c#
public abstract class Node
{
    public abstract CellResult Evaluate(IEvaluationContext context);
}
```
IEvaluationContext is responsible for providing referenced cells' values to the evaluation, the interface is the following:
```c#
public interface IEvaluationContext
{
    public CellResult? GetExternalCellValue(string url);
    public CellResult? GetCellValue(CellId identifier);
    public void UpdateContext(Cell cell);
}
```
Expression in reverse polish notation can be easily converted to AST with the following algorithm:
```c#
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
```

## Functions implementation
Implementing functions with arbitrary number of parameters is a complicated task. I decided to implement it by means of unary and binary operators. For instance MAX function can be considered as an unary operator on range of values. To serve this purpose a new cell result type was created - RangeCellResult which contains list of CellResult. Also I introduced comma operator (",") as a binary operator. This idea is quite similar to collections implementation in functional languages (where thay are recirsive), as a result MAX(1,2,3) during AST building is converted into the next tree:
```
Max
-> Comma
   -> 1
    -> Comma
      -> 2
      -> Comma
        -> 3
```
Then maximum AST node implementation can be the following:
```c#
public class MaxOperatorNode : UnaryOperatorNode
{
    public MaxOperatorNode(Node node) : base(node)
    {
    }

    public override CellResult Evaluate(IEvaluationContext context)
    {
        var results = Node.Evaluate(context);
        if (results is not CellRangeResult rangeResult || !rangeResult.Results.Any())
        {
            return new ErrorResult();
        }
        
        if (rangeResult.Results.Any(x => x is not DoubleResult))
        {
            return new ErrorResult();
        }

        var sum = rangeResult.Results.Cast<DoubleResult>().Select(x => x.Value).Max();
        return new DoubleResult(sum);
    }
}
```

## External references implementation
To implenent external references a new service was added which is responsible for extraction of cells values based on http requests, the interface is the following:
```c#
public interface IExternalCellService
{
    public CellResult? GetExternalCellResult(Uri uri);
}
```
During the implementation of http-based communication there is the possibility of many client allocation (which is resource expensive), that is why a polling was used to effectively manage http clients. External references are used in function by means of EXTERNAL_REF operator, for instance:
```
val1=min(5, external_ref('http://localhost/api/v1/1/1/'))
```

## WebHooks API implementation
An API supports method to subscribe on the cell and to trigger post request on some webhook_url, subscriptions are persisted in the database and are stored with the following format:
```json
{
    "CellId": "testcell",
    "SheetId": "testsheet",
    "WebHookUrls": ["http://localhost:8080/hello"]
}
```
Webhooks is triggered when cell is modified explicitly, via api call or is transitively updated when some of its dependencies is updated.

## Data Format
Excel is all about calculating values in formulas in cells and it is nesessary to store cells dependencies graph to recalculate the formulas effectively. For us it is necessary to obtain the cells that will be affected if the value in particular cell changes and also to obtain cells dependencies. To capture this requirements the following data structure is used:

```json
{
    "CellId": "testcell",
    "SheetId": "testsheet",
    "Value": "=anothercell+5",
    "Result": "15",
    "DependsOn": ["anothercell"]
}
```
With such model it is possible to recursively search dependencies graph with mongodb $graphLookup function. To increase the performance for the application the following indexes were added:
- compound { SheetId: 1, DependsOn: 1}
- compound unique { SheetId: 1, CellId: 1}

## Dependent Cells Recalculation
When cell is updated the recalculate process is triggered. It consists of the following steps:
- load cells that are affected by change of cells value (these values are topologically sorted to correctly evaluate cells)
- load dependencies of that cells and populate evaluation context
- for each cell in affected: 
  - calculate new value
  - add to context to correctly recalculate next affected cell
- save updated cell to sheet
```c#
 public async Task<IEnumerable<Cell>> Recalculate(Cell cell)
{
    var recalculatedCells = new List<Cell>();
    var affectedCells = (await cell.Sheet.GetAffectedCells(cell)).ToList();
    affectedCells.Insert(0, cell);
        
    var cellsDependencies = await cell.Sheet.GetCellsDependencies(affectedCells);
    var evaluationContext = EvaluationContext.Create(cellsDependencies);
            
    foreach (var affectedCell in affectedCells)
    {
        var result = _formulaEngine.Evaluate(evaluationContext, affectedCell);
        var recalculatedCell = affectedCell with { Result = result };
        recalculatedCells.Add(recalculatedCell);
        evaluationContext.UpdateContext(recalculatedCell);
    }
    return recalculatedCells;
}
```
## How To Start Application
To start an application please use the following command:
```
docker compose up --build
```

## Testing
To run unit tests in docker container please use the following command: 
```
docker exec excel-api dotnet test -l "console;verbosity=normal" /app/tests/Excel.Tests.Unit.dll
```

To run integration tests in docker container please use the following command:
```
docker exec excel-api dotnet test -l "console;verbosity=normal" /app/integration-tests/Excel.Tests.Integration.dll
```

## Corner Cases Covered
- invalid formula (according to operators structure) returns ERROR
- self-referencing formula returns ERROR
- operations on string except "+" (concatenation) produce ERROR
- "+" operation on string and number produces string (number is converted to string)
- division by zero in formula produces ERROR
- unary minus is supported
- formula referencing to unknown produces ERROR
- external reference on endpoint that produces ERROR result returns ERROR
- min, max, avg, sum operations when there are no parameters return ERROR
- min, max, avg, sum operations support any number of parameters, function calls can be nested "max(1, avg(a, b+d, c), min(d/2, e))"
- webhooks are triggered not only for the directly modified cell, but also for the dependent cells

## Further Steps
With the developed formula engine it is quite easy to support more functions and data types that are available in Excel. Also in future I would consider more the performance of formula engine, it will make sense to optimize it if possible.