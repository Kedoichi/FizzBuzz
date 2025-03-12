using System;
using System.Threading.Tasks;
using FizzBuzzGame.Core.Models;

namespace FizzBuzzGame.Core.Interfaces
{
    public interface IGameSessionRepository
    {
        Task<GameSession> CreateSessionAsync(GameSession session);
        Task<GameSession?> GetSessionByIdAsync(Guid id);
        Task UpdateSessionAsync(GameSession session);
        Task AddRoundToSessionAsync(GameRound round);
        Task EndSessionAsync(Guid id);
    }
}
