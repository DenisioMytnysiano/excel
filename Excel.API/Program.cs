using Excel.Core.FormulaEngine;
using Excel.Core.FormulaEngine.Lexer;
using Excel.Core.Services.Implementation;
using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.ExternalCelService;
using Excel.Infrastructure.Notification;
using Excel.Infrastructure.Sheet;
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddHttpClient();
builder.Services.AddMongoSheetStore(builder.Configuration);
builder.Services.AddSingleton<ILexer, Lexer>();
builder.Services.AddSingleton<IFormulaEngine, FormulaEngine>();
builder.Services.AddSingleton<ICellUpdateNotificationService, CellNotificationService>();
builder.Services.AddScoped<IRecalculateService, RecalculateService>();
builder.Services.AddScoped<IExternalCellService, ExternalCellService>();

var app = builder.Build();
app.UseFastEndpoints(options =>
{
    options.Endpoints.RoutePrefix = "api";
    options.Versioning.PrependToRoute = true;
    options.Versioning.DefaultVersion = 1;
    options.Versioning.Prefix = "v";
});
app.Run();

namespace Excel.API
{
    public partial class Program { }
}