using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Excel.Infrastructure.Mongo;

public static class Extensions
{
    public static void AddMongoStore(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<MongoOptions>(configuration.GetSection("Mongo"));
        serviceCollection.AddSingleton<IMongoClient>(x =>
        {
            var mongoOptions = x.GetRequiredService<IOptions<MongoOptions>>().Value;
            var builder = new MongoUrlBuilder
            {
                Username = mongoOptions.UserName,
                Password = mongoOptions.Password,
                Server = new MongoServerAddress(mongoOptions.Host, mongoOptions.Port)
            };
            return new MongoClient(new MongoUrl(builder.ToString()));
        });
    }
}