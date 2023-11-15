using System.Net;
using System.Net.Http.Json;
using Excel.API;
using Excel.API.v1.Cells.Responses;
using Excel.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Excel.Tests.Integration;

public class CellsApiTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ISheetProvider _sheetProvider;
    
    public CellsApiTests(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        _client = factory.CreateClient();
        _sheetProvider = scope.ServiceProvider.GetRequiredService<ISheetProvider>();
    }

    [Fact]
    public async Task AddCell_Returns_422StatusCode_When_FormulaCompilesToError()
    {
        var sheet = Guid.NewGuid().ToString("N");;
        var cell = Guid.NewGuid().ToString("N");
        
        var response = await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = $"={cell}+5"}));
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
    
    [Fact]
    public async Task AddCell_Returns_CellWithError_When_FormulaCompilesToError()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        
        var response = await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = $"={cell}+5"}));

        var cellResponse = await response.Content.ReadFromJsonAsync<AddCellResponse>();
        Assert.Equal($"={cell}+5", cellResponse?.Value);
        Assert.Equal("ERROR", cellResponse?.Result);
    }
    [Fact]
    public async Task AddCell_Returns_201StatusCode_CreatedCellWithEvaluatedValue_When_FormulaIsValid()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell1 = Guid.NewGuid().ToString("N");
        var cell2 = Guid.NewGuid().ToString("N");
        var cell3 = Guid.NewGuid().ToString("N");
        
        await _client.PostAsync($"/api/v1/{sheet}/{cell1}", JsonContent.Create(new {Value = "5"}));
        await _client.PostAsync($"/api/v1/{sheet}/{cell2}", JsonContent.Create(new {Value = "5"}));
        var response = await _client.PostAsync($"/api/v1/{sheet}/{cell3}", JsonContent.Create(new {Value = $"={cell1}+{cell2}/5"}));
        await ClearSheet(SheetId.Create(sheet));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task AddCell_Returns_CreatedCellWithEvaluatedValue_When_FormulaIsValid()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell1 = Guid.NewGuid().ToString("N");
        var cell2 = Guid.NewGuid().ToString("N");
        var cell3 = Guid.NewGuid().ToString("N");
        
        await _client.PostAsync($"/api/v1/{sheet}/{cell1}", JsonContent.Create(new {Value = "5"}));
        await _client.PostAsync($"/api/v1/{sheet}/{cell2}", JsonContent.Create(new {Value = "5"}));
        var response = await _client.PostAsync($"/api/v1/{sheet}/{cell3}", JsonContent.Create(new {Value = $"={cell1}+{cell2}/5"}));

        var cellResponse = await response.Content.ReadFromJsonAsync<AddCellResponse>();
        await ClearSheet(SheetId.Create(sheet));
        Assert.Equal("6", cellResponse?.Result);
    }
    [Fact]
    public async Task AddCell_AddCellToSheetAndCanBeObtainedLater_When_FormulaIsValid()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        
        await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = "10"}));

        var response = await _client.GetAsync($"/api/v1/{sheet}/{cell}");
        var cellResponse = await response.Content.ReadFromJsonAsync<AddCellResponse>();
        await ClearSheet(SheetId.Create(sheet));

        Assert.Equal("10", cellResponse?.Result);
        Assert.Equal("10", cellResponse?.Value);
    }

    [Fact]
    public async Task AddCell_RecalculatesDependentCells_When_UpdateCellIsReferencedInOtherCells()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell1 = Guid.NewGuid().ToString("N");
        var cell2 = Guid.NewGuid().ToString("N");
        var cell3 = Guid.NewGuid().ToString("N");
        
        await _client.PostAsync($"/api/v1/{sheet}/{cell1}", JsonContent.Create(new {Value = "5"}));
        await _client.PostAsync($"/api/v1/{sheet}/{cell2}", JsonContent.Create(new {Value = $"={cell1}+5"}));
        await _client.PostAsync($"/api/v1/{sheet}/{cell3}", JsonContent.Create(new {Value = $"={cell2}/5"}));
        
        await _client.PostAsync($"/api/v1/{sheet}/{cell1}", JsonContent.Create(new {Value = "15"}));

        var recalculatedCell2 = await (await _client.GetAsync($"/api/v1/{sheet}/{cell2}")).Content.ReadFromJsonAsync<AddCellResponse>();
        var recalculatedCell3 = await (await _client.GetAsync($"/api/v1/{sheet}/{cell3}")).Content.ReadFromJsonAsync<AddCellResponse>();
        await ClearSheet(SheetId.Create(sheet));
        Assert.Equal("20", recalculatedCell2?.Result);
        Assert.Equal("4", recalculatedCell3?.Result);
    }
    
    [Fact]
    public async Task GetCell_Returns_404StatusCode_When_CellIsNotPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        var response = await _client.GetAsync($"/api/v1/{sheet}/{cell}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetCell_Returns_200StatusCode_When_CellIsPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = "5"}));
        var response = await _client.GetAsync($"/api/v1/{sheet}/{cell}");
        await ClearSheet(SheetId.Create(sheet));
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetCell_Returns_Cell_When_CellIsPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = "5"}));
        var foundCell = await (await _client.GetAsync($"/api/v1/{sheet}/{cell}")).Content.ReadFromJsonAsync<GetCellResponse>();
        await ClearSheet(SheetId.Create(sheet));

        Assert.Equal("5", foundCell?.Result);
        Assert.Equal("5", foundCell?.Value);
    }
    
    [Fact]
    public async Task GetSheet_Returns_404StatusCode_When_SheetIsNotPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var response = await _client.GetAsync($"/api/v1/{sheet}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetSheet_Returns_200StatusCode_When_SheetIsPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell = Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/v1/{sheet}/{cell}", JsonContent.Create(new {Value = "5"}));
        var response = await _client.GetAsync($"/api/v1/{sheet}");
        await ClearSheet(SheetId.Create(sheet));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetSheet_Returns_AllCellsOnSheet_When_SheetIsPresent()
    {
        var sheet = Guid.NewGuid().ToString("N");
        var cell1 = Guid.NewGuid().ToString("N");
        var cell2 = Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/v1/{sheet}/{cell1}", JsonContent.Create(new {Value = "5"}));
        await _client.PostAsync($"/api/v1/{sheet}/{cell2}", JsonContent.Create(new {Value = "5"}));
        var response = JsonConvert.DeserializeObject<GetSheetResponse>(await (await _client.GetAsync($"/api/v1/{sheet}")).Content.ReadAsStringAsync());
        await ClearSheet(SheetId.Create(sheet));

        Assert.True(response!.Count == 2);
    }

    private async Task ClearSheet(SheetId sheetId)
    {
        var sheet = _sheetProvider.GetSheet(sheetId);
        await sheet.Delete();
    }
}