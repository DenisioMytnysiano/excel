using Excel.Core.Entities;
using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.Notification;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NSubstitute;

namespace Excel.Tests.Integration;

public class CellsNotificationTests: IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly ICellUpdateNotificationService _notificationService;
    private readonly MockMessageHandler _messageHandler;
    
    public CellsNotificationTests(IntegrationTestWebAppFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var mongo = scope.ServiceProvider.GetRequiredService<IMongoClient>();

        var factoryMock = Substitute.For<IHttpClientFactory>();
        _messageHandler = new MockMessageHandler();
        var client = new HttpClient(_messageHandler);
        factoryMock.CreateClient(Arg.Any<string>()).ReturnsForAnyArgs(client);
        _notificationService = new CellNotificationService(mongo, factoryMock);
    }

    [Fact]
    public void WebHookUrlIsTriggerred_When_SubscriptionWasRegistered()
    {
        var sheetMock = Substitute.For<ISheet>();
        sheetMock.GetSheetId().Returns(SheetId.Create("1"));
        var cell = new Cell(CellId.Create("1"), CellValue.Parse("1"), CellResult.Parse("1"), sheetMock);

        var webhookUrl = new Uri("http://localhost:2020");
        _notificationService.Subscribe(cell, webhookUrl);
        _notificationService.Notify(cell);

        Assert.Equal(1, _messageHandler.InvokedTimes);
    }
    
    [Fact]
    public void WebHookUrlIsNotTriggerred_When_SubscriptionWasNotFound()
    {
        var sheetMock = Substitute.For<ISheet>();
        sheetMock.GetSheetId().Returns(SheetId.Create("1"));
        var cell1 = new Cell(CellId.Create("1"), CellValue.Parse("1"), CellResult.Parse("1"), sheetMock);
        var cell2 = new Cell(CellId.Create("2"), CellValue.Parse("1"), CellResult.Parse("1"), sheetMock);

        var webhookUrl = new Uri("http://localhost:2020");
        _notificationService.Subscribe(cell1, webhookUrl);
        _notificationService.Notify(cell2);

        Assert.Equal(0, _messageHandler.InvokedTimes);
    }
    
    private class MockMessageHandler : HttpMessageHandler
    {
        public int InvokedTimes { get; set; } = 0;

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InvokedTimes += 1;
            return new HttpResponseMessage();
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InvokedTimes += 1;
            return new HttpResponseMessage();
        }
    }
}