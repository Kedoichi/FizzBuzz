using System;
using System.Threading.Tasks;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using FizzBuzzGame.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace FizzBuzzGame.Infrastructure.Repositories
{
    public class MongoGameSessionRepository : IGameSessionRepository
    {
        private readonly IMongoCollection<GameSession> _sessionsCollection;
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly ILogger<MongoGameSessionRepository> _logger;

        public MongoGameSessionRepository(MongoDbSettings settings, ILogger<MongoGameSessionRepository> logger)
        {
            _logger = logger;
            try
            {
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);
                _sessionsCollection = database.GetCollection<GameSession>(settings.GameSessionsCollectionName);
                _gamesCollection = database.GetCollection<Game>(settings.GamesCollectionName);

                _logger.LogInformation("Successfully connected to MongoDB: {Database}/{Collection}",
                    settings.DatabaseName, settings.GameSessionsCollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB");
                throw;
            }
        }

        public async Task<GameSession> CreateSessionAsync(GameSession session)
        {
            try
            {
                // Ensure a new ID is generated
                session.Id = Guid.NewGuid();
                session.StartTime = DateTime.UtcNow;
                session.CorrectAnswers = 0;
                session.IncorrectAnswers = 0;
                session.UsedNumbers = new List<int>();
                session.Rounds = new List<GameRound>();

                // Get the associated game for this session
                session.Game = await _gamesCollection.Find(g => g.Id == session.GameId).FirstOrDefaultAsync();

                await _sessionsCollection.InsertOneAsync(session);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game session for game: {GameId}", session.GameId);
                throw;
            }
        }

        public async Task<GameSession?> GetSessionByIdAsync(Guid id)
        {
            try
            {
                var session = await _sessionsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();

                if (session != null && session.Game == null)
                {
                    // Load the game if it's not populated
                    session.Game = await _gamesCollection.Find(g => g.Id == session.GameId).FirstOrDefaultAsync();
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game session: {SessionId}", id);
                throw;
            }
        }

        public async Task UpdateSessionAsync(GameSession session)
        {
            try
            {
                await _sessionsCollection.ReplaceOneAsync(s => s.Id == session.Id, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game session: {SessionId}", session.Id);
                throw;
            }
        }

        public async Task AddRoundToSessionAsync(GameRound round)
        {
            try
            {
                round.Id = Guid.NewGuid();

                // Get the current session
                var session = await _sessionsCollection.Find(s => s.Id == round.GameSessionId).FirstOrDefaultAsync();

                if (session != null)
                {
                    // Add the number to used numbers
                    session.UsedNumbers.Add(round.Number);

                    // Update score
                    if (round.IsCorrect)
                    {
                        session.CorrectAnswers++;
                    }
                    else
                    {
                        session.IncorrectAnswers++;
                    }

                    // Add the round to the session
                    session.Rounds.Add(round);

                    // Update the session in the database
                    await _sessionsCollection.ReplaceOneAsync(s => s.Id == session.Id, session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding round to game session: {SessionId}", round.GameSessionId);
                throw;
            }
        }

        public async Task EndSessionAsync(Guid id)
        {
            try
            {
                var session = await _sessionsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();

                if (session != null)
                {
                    session.EndTime = DateTime.UtcNow;
                    await _sessionsCollection.ReplaceOneAsync(s => s.Id == id, session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending game session: {SessionId}", id);
                throw;
            }
        }
    }
}
