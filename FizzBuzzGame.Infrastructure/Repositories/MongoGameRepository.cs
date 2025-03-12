using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using FizzBuzzGame.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace FizzBuzzGame.Infrastructure.Repositories
{
    public class MongoGameRepository : IGameRepository
    {
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly ILogger<MongoGameRepository> _logger;

        public MongoGameRepository(MongoDbSettings settings, ILogger<MongoGameRepository> logger)
        {
            _logger = logger;
            try
            {
                var mongoUrl = MongoUrl.Create(settings.ConnectionString);
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);
                _gamesCollection = database.GetCollection<Game>(settings.GamesCollectionName);

                _logger.LogInformation("Successfully connected to MongoDB: {Database}/{Collection}",
                    settings.DatabaseName, settings.GamesCollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB");
                throw;
            }
        }

        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            try
            {
                return await _gamesCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all games");
                throw;
            }
        }

        public async Task<Game?> GetGameByIdAsync(Guid id)
        {
            try
            {
                return await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game by ID: {GameId}", id);
                throw;
            }
        }

        public async Task<Game?> GetGameByNameAsync(string name)
        {
            try
            {
                return await _gamesCollection.Find(g => g.Name == name).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game by name: {GameName}", name);
                throw;
            }
        }

        public async Task<Game> CreateGameAsync(Game game)
        {
            try
            {
                // Ensure a new ID is generated
                game.Id = Guid.NewGuid();
                game.DateCreated = DateTime.UtcNow;

                // Assign IDs to rules
                foreach (var rule in game.Rules)
                {
                    rule.Id = Guid.NewGuid();
                    rule.GameId = game.Id;
                }

                await _gamesCollection.InsertOneAsync(game);
                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game: {GameName}", game.Name);
                throw;
            }
        }

        public async Task UpdateGameAsync(Game game)
        {
            try
            {
                // Update existing rules with game ID
                foreach (var rule in game.Rules)
                {
                    if (rule.Id == Guid.Empty)
                        rule.Id = Guid.NewGuid();

                    rule.GameId = game.Id;
                }

                await _gamesCollection.ReplaceOneAsync(g => g.Id == game.Id, game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game: {GameId}", game.Id);
                throw;
            }
        }

        public async Task DeleteGameAsync(Guid id)
        {
            try
            {
                await _gamesCollection.DeleteOneAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game: {GameId}", id);
                throw;
            }
        }
    }
}
