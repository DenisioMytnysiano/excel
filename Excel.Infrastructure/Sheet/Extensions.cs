using Excel.Core.Services.Interfaces;
using Excel.Infrastructure.Mongo;
using Excel.Infrastructure.Sheet.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Excel.Infrastructure.Sheet;

public static class Extensions
{
    public static void AddMongoSheetStore(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMongoStore(configuration);
        serviceCollection.AddHostedService<MongoSheetSetup>();
        serviceCollection.AddSingleton<MongoCellConverter>();
        serviceCollection.AddScoped<ISheetProvider, MongoSheetProvider>();
    }
}