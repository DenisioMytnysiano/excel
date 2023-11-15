using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace Excel.Tests.Integration;

public class MongoSheetTests: IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly ISheetProvider _sheetProvider;
    
    public MongoSheetTests(IntegrationTestWebAppFactory integrationTestWebAppFactory)
    {
        using var scope = integrationTestWebAppFactory.Services.CreateScope();
        _sheetProvider = scope.ServiceProvider.GetRequiredService<ISheetProvider>();
    }

    [Fact]
    public void GetSheetId_Returns_SheetIdUsedInSheetProvider()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        Assert.Equal(sheetId, sheet.GetSheetId());
    }

    [Fact]
    public async Task GetCells_Returns_EmptyList_When_ThereAreNoCellsOnTheSheet()
    {
        var cellIds = new List<CellId> { CellId.Create("test1"), CellId.Create("test2") };
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cells = await sheet.GetCells(cellIds);
        Assert.Empty(cells);
    }
    
    [Fact]
    public async Task GetCells_Returns_EmptyList_When_CellIdsAreEmpty()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cells = new List<Cell>
        {
            new(CellId.Create("test1"), new DoubleValue(10.0), new DoubleResult(10.0), sheet),
            new(CellId.Create("test2"), new DoubleValue(10.0), new DoubleResult(10.0), sheet),

        };
        await sheet.UpdateCells(cells);
        var foundCells = await sheet.GetCells(Array.Empty<CellId>());
        await sheet.Delete();
        Assert.Empty(foundCells);
    }
    [Fact]
    public async Task GetCells_Returns_CellsWhichExistInSheet_When_NotAllCellIdsAreFound()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var cellId = CellId.Create("test1");
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cells = new List<Cell>{ new(cellId, new DoubleValue(10.0), new DoubleResult(10.0), sheet) };
        await sheet.UpdateCells(cells);
        
        var foundCells = (await sheet.GetCells(new List<CellId>{cellId, CellId.Create("test2")})).ToList();
        await sheet.Delete();
        Assert.Single(foundCells);
        Assert.Equal(cellId, foundCells.Single().Id);
    }

    [Fact]
    public async Task GetCells_Returns_EmptyList_When_ThereAreNoCellsOnSheet()
    {
        var otherSheetId = SheetId.Create(Guid.NewGuid().ToString());
        var cellId = CellId.Create("test1");
        var otherSheet = _sheetProvider.GetSheet(otherSheetId);
        await otherSheet.UpdateCells(new List<Cell>{ new(cellId, new DoubleValue(10.0), new DoubleResult(10.0), otherSheet) });
        
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cells = await sheet.GetCells();
        await otherSheet.Delete();
        Assert.Empty(cells);
    }
    
    [Fact]
    public async Task GetCells_Returns_AllCells_When_ThereAreCellsOnSheet()
    {
        var otherSheetId = SheetId.Create(Guid.NewGuid().ToString());
        var cellId = CellId.Create("test1");
        var otherSheet = _sheetProvider.GetSheet(otherSheetId);
        await otherSheet.UpdateCells(new List<Cell>{ new(cellId, new DoubleValue(10.0), new DoubleResult(10.0), otherSheet) });
        
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cellsToInsert = new List<Cell>
        {
            new(CellId.Create("test2"), new DoubleValue(10.0), new DoubleResult(10.0), otherSheet),
            new(CellId.Create("test3"), new DoubleValue(10.0), new DoubleResult(10.0), otherSheet)
        };
        await sheet.UpdateCells(cellsToInsert);

        var cells = (await sheet.GetCells()).ToList();
        await sheet.Delete();
        await otherSheet.Delete();
        Assert.True(cells.Select(x => x.Id).SequenceEqual(cellsToInsert.Select(y => y.Id)));
    }

    [Fact]
    public async Task GetCellsDependencies_Returns_CellReferences_When_CellIsFormula()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var cellId = CellId.Create("test");
        var sheet = _sheetProvider.GetSheet(sheetId);
        
        var expectedDependencies = new List<Cell>()
        {
            new(CellId.Create("a"), new DoubleValue(5.0), new DoubleResult(5.0), sheet),
            new(CellId.Create("b"), new DoubleValue(3.0), new DoubleResult(3.0), sheet),
        };
        var formulaCell = new Cell(cellId, new FormulaValue("a+b*10"), new DoubleResult(35.0), sheet);
        var cells = new List<Cell>();
        cells.AddRange(expectedDependencies);
        cells.Add(formulaCell);
        await sheet.UpdateCells(cells);
        
        var dependencies = await sheet.GetCellsDependencies(new []{ formulaCell });
        await sheet.Delete();
        Assert.True(expectedDependencies.Select(x => x.Id).SequenceEqual(dependencies.Select(x => x.Id)));
    }
    
    [Fact]
    public async Task GetCellsDependencies_Returns_EmptyList_When_CellIsNotFormula()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);

        var cell = new Cell(CellId.Create("a"), new DoubleValue(5.0), new DoubleResult(5.0), sheet);
        var cells = new List<Cell>()
        {
            cell,
            new(CellId.Create("b"), new DoubleValue(3.0), new DoubleResult(3.0), sheet),
            new(CellId.Create("test"), new FormulaValue("a+b*10"), new DoubleResult(35.0), sheet)
        };
        await sheet.UpdateCells(cells);
        
        var dependencies = await sheet.GetCellsDependencies(new []{ cell });
        await sheet.Delete();
        Assert.Empty(dependencies);
    }

    [Fact]
    public async Task GetAffectedCells_Returns_AllNodesWhereDependenciesContainsCell()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var expectedDependencyOrder = new List<CellId>
        {
            CellId.Create("test3"),
            CellId.Create("test4")
        };
        
        var baseCell = new Cell(CellId.Create("a"), new DoubleValue(5.0), new DoubleResult(5.0), sheet);
        var cells = new List<Cell>()
        {
            baseCell,
            new(CellId.Create("b"), new DoubleValue(3.0), new DoubleResult(3.0), sheet),
            new(CellId.Create("test2"), new FormulaValue("test*10"), new DoubleResult(35.0), sheet),
            new(CellId.Create("test3"), new FormulaValue("a*10"), new DoubleResult(35.0), sheet),
            new(CellId.Create("test4"), new FormulaValue("test3*10"), new DoubleResult(35.0), sheet)
        };
        await sheet.UpdateCells(cells);
        
        var dependencies = await sheet.GetAffectedCells(baseCell);
        await sheet.Delete();
        AssertSequencesEqual(dependencies.Select(x => x.Id), expectedDependencyOrder);
    }
    
    [Fact]
    public async Task GetAffectedCells_Returns_EmptyList_When_CellIsNotUsedInAnyOtherCell()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var standaloneCell = new Cell(CellId.Create("b"), new DoubleValue(3.0), new DoubleResult(3.0), sheet);
        var cells = new List<Cell>()
        {
            standaloneCell,
            new(CellId.Create("a"), new DoubleValue(5.0), new DoubleResult(5.0), sheet),
            new(CellId.Create("test"), new FormulaValue("a*10"), new DoubleResult(35.0), sheet),
            new(CellId.Create("test2"), new FormulaValue("test*10"), new DoubleResult(35.0), sheet),
            new(CellId.Create("test3"), new FormulaValue("a*10"), new DoubleResult(35.0), sheet),
            new(CellId.Create("test4"), new FormulaValue("test3*10"), new DoubleResult(35.0), sheet)
        };
        await sheet.UpdateCells(cells);
        
        var dependencies = await sheet.GetAffectedCells(standaloneCell);
        await sheet.Delete();
        Assert.Empty(dependencies);
    }
    
    [Fact]
    public async Task UpdateCells_UpdateCellsValue_When_CellIsAlreadyPresentOnTheSheet()
    {
        var sheetId = SheetId.Create(Guid.NewGuid().ToString());
        var sheet = _sheetProvider.GetSheet(sheetId);
        var cell = new Cell(CellId.Create("a"), new DoubleValue(5.0), new DoubleResult(5.0), sheet);
        await sheet.UpdateCells(new []{ cell });
        await sheet.UpdateCells(new []{ cell with { Result = new DoubleResult(10)} });
        var updatedCell = await sheet.Get(cell.Id);
        await sheet.Delete();
        Assert.NotEqual(cell.Result, updatedCell?.Result);
    }

    private void AssertSequencesEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
    {
        if (first.Count() != second.Count())
        {
            throw new AssertActualExpectedException(first, second, "Sequences have different length");
        }

        if (!first.Zip(second).All(x => x.First.Equals(x.Second)))
        {
            throw new AssertActualExpectedException(first, second, "Sequences have different elements");
        } 
    }
}