using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Services;
using FizzBuzzGame.Infrastructure.Data;
using FizzBuzzGame.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace FizzBuzzGame.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Guid serialization BEFORE any MongoDB operations
            ConfigureMongoDbSerialization();

            // Manually bind MongoDbSettings
            var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

            if (mongoDbSettings == null)
            {
                throw new ArgumentNullException(nameof(mongoDbSettings), "MongoDbSettings section is missing in configuration.");
            }

            services.AddSingleton(mongoDbSettings);

            // Register MongoDB Client
            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoDbSettings.ConnectionString));

            // Register Repositories
            services.AddSingleton<IGameRepository, MongoGameRepository>();
            services.AddSingleton<IGameSessionRepository, MongoGameSessionRepository>();

            // Register GameLogicService
            services.AddSingleton<GameLogicService>();

            return services;
        }

        private static void ConfigureMongoDbSerialization()
        {
            // Configure Guid serialization to use Standard representation
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
    }
}
