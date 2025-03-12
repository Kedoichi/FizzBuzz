using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzBuzzGame.API.DTOs;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FizzBuzzGame.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepository _gameRepository;
        private readonly ILogger<GamesController> _logger;

        public GamesController(IGameRepository gameRepository, ILogger<GamesController> logger)
        {
            _gameRepository = gameRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all available games
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGames()
        {
            try
            {
                var games = await _gameRepository.GetAllGamesAsync();

                return Ok(games.Select(game => new GameDto
                {
                    Id = game.Id,
                    Name = game.Name,
                    Author = game.Author,
                    DateCreated = game.DateCreated,
                    Rules = game.Rules.Select(rule => new GameRuleDto
                    {
                        Id = rule.Id,
                        Divisor = rule.Divisor,
                        ReplaceWord = rule.ReplaceWord,
                        SortOrder = rule.SortOrder
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games");
                return StatusCode(500, "An error occurred while retrieving games.");
            }
        }

        /// <summary>
        /// Gets a specific game by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGame(Guid id)
        {
            try
            {
                var game = await _gameRepository.GetGameByIdAsync(id);

                if (game == null)
                {
                    _logger.LogWarning("Game with ID {GameId} not found", id);
                    return NotFound();
                }

                return Ok(new GameDto
                {
                    Id = game.Id,
                    Name = game.Name,
                    Author = game.Author,
                    DateCreated = game.DateCreated,
                    Rules = game.Rules.Select(rule => new GameRuleDto
                    {
                        Id = rule.Id,
                        Divisor = rule.Divisor,
                        ReplaceWord = rule.ReplaceWord,
                        SortOrder = rule.SortOrder
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game with ID {GameId}", id);
                return StatusCode(500, $"An error occurred while retrieving game with ID {id}.");
            }
        }

        /// <summary>
        /// Creates a new game with rules
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GameDto>> CreateGame(CreateGameDto createGameDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate game rules
                if (createGameDto.Rules == null || !createGameDto.Rules.Any())
                {
                    return BadRequest("At least one rule must be provided.");
                }

                // Check for duplicate divisors
                var divisors = createGameDto.Rules.Select(r => r.Divisor).ToList();
                if (divisors.Count != divisors.Distinct().Count())
                {
                    return BadRequest("Duplicate divisors are not allowed.");
                }

                // Check for valid rule values
                foreach (var rule in createGameDto.Rules)
                {
                    if (rule.Divisor <= 0)
                    {
                        return BadRequest($"Divisor must be positive: {rule.Divisor}");
                    }

                    if (string.IsNullOrWhiteSpace(rule.ReplaceWord))
                    {
                        return BadRequest("Replace word cannot be empty.");
                    }
                }

                // Check if a game with this name already exists
                var existingGame = await _gameRepository.GetGameByNameAsync(createGameDto.Name);
                if (existingGame != null)
                {
                    return BadRequest($"A game with the name '{createGameDto.Name}' already exists.");
                }

                // Create the new game
                var game = new Game
                {
                    Name = createGameDto.Name,
                    Author = createGameDto.Author,
                    DateCreated = DateTime.UtcNow,
                    Rules = createGameDto.Rules.Select((rule, index) => new GameRule
                    {
                        Divisor = rule.Divisor,
                        ReplaceWord = rule.ReplaceWord,
                        SortOrder = rule.SortOrder != 0 ? rule.SortOrder : index
                    }).ToList()
                };

                await _gameRepository.CreateGameAsync(game);

                _logger.LogInformation("Game created successfully: {GameName}", game.Name);

                return CreatedAtAction(nameof(GetGame), new { id = game.Id }, new GameDto
                {
                    Id = game.Id,
                    Name = game.Name,
                    Author = game.Author,
                    DateCreated = game.DateCreated,
                    Rules = game.Rules.Select(rule => new GameRuleDto
                    {
                        Id = rule.Id,
                        Divisor = rule.Divisor,
                        ReplaceWord = rule.ReplaceWord,
                        SortOrder = rule.SortOrder
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                _logger.LogError(ex, "Detailed error creating game: {GameName}. Exception: {ExceptionMessage}",
                    createGameDto.Name, ex.ToString());

                // If using development environment, you might want to return more details
                return StatusCode(500, $"An error occurred while creating the game: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing game
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(Guid id, CreateGameDto updateGameDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var game = await _gameRepository.GetGameByIdAsync(id);

                if (game == null)
                {
                    _logger.LogWarning("Game with ID {GameId} not found for update", id);
                    return NotFound();
                }

                // Validate game rules
                if (updateGameDto.Rules == null || !updateGameDto.Rules.Any())
                {
                    return BadRequest("At least one rule must be provided.");
                }

                // Check for duplicate divisors
                var divisors = updateGameDto.Rules.Select(r => r.Divisor).ToList();
                if (divisors.Count != divisors.Distinct().Count())
                {
                    return BadRequest("Duplicate divisors are not allowed.");
                }

                // Check if the updated name is already used by another game
                if (game.Name != updateGameDto.Name)
                {
                    var existingGame = await _gameRepository.GetGameByNameAsync(updateGameDto.Name);
                    if (existingGame != null && existingGame.Id != id)
                    {
                        return BadRequest($"A game with the name '{updateGameDto.Name}' already exists.");
                    }
                }

                // Update the game properties
                game.Name = updateGameDto.Name;
                game.Author = updateGameDto.Author;

                // Replace rules with new ones
                game.Rules = updateGameDto.Rules.Select((rule, index) => new GameRule
                {
                    Id = Guid.NewGuid(), // Create new IDs for rules
                    GameId = game.Id,
                    Divisor = rule.Divisor,
                    ReplaceWord = rule.ReplaceWord,
                    SortOrder = rule.SortOrder != 0 ? rule.SortOrder : index // Use provided SortOrder or index as default
                }).ToList();

                await _gameRepository.UpdateGameAsync(game);

                _logger.LogInformation("Game updated successfully: {GameName}", game.Name);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game with ID {GameId}", id);
                return StatusCode(500, $"An error occurred while updating game with ID {id}.");
            }
        }

        /// <summary>
        /// Deletes a game
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(Guid id)
        {
            try
            {
                var game = await _gameRepository.GetGameByIdAsync(id);

                if (game == null)
                {
                    _logger.LogWarning("Game with ID {GameId} not found for deletion", id);
                    return NotFound();
                }

                await _gameRepository.DeleteGameAsync(id);

                _logger.LogInformation("Game deleted successfully: {GameName}", game.Name);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game with ID {GameId}", id);
                return StatusCode(500, $"An error occurred while deleting game with ID {id}.");
            }
        }
    }
}
