using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FizzBuzzGame.Core.Models;

namespace FizzBuzzGame.Core.Interfaces
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<Game?> GetGameByIdAsync(Guid id);
        Task<Game?> GetGameByNameAsync(string name);
        Task<Game> CreateGameAsync(Game game);
        Task UpdateGameAsync(Game game);
        Task DeleteGameAsync(Guid id);
    }
}
