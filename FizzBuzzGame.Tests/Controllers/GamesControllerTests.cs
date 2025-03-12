using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzBuzzGame.API.Controllers;
using FizzBuzzGame.API.DTOs;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FizzBuzzGame.Tests.Controllers
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<ILogger<GamesController>> _mockLogger;
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockLogger = new Mock<ILogger<GamesController>>();
            _controller = new GamesController(_mockGameRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetGames_ReturnsOkResult_WithListOfGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game
                {
                    Id = Guid.NewGuid(),
                    Name = "FizzBuzz",
                    Author = "Test Author",
                    DateCreated = DateTime.UtcNow,
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                        new GameRule { Id = Guid.NewGuid(), Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                    }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetAllGamesAsync())
                .ReturnsAsync(games);

            // Act
            var result = await _controller.GetGames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);
            Assert.Single(returnedGames);
            Assert.Equal(games[0].Name, returnedGames.First().Name);
            Assert.Equal(2, returnedGames.First().Rules.Count);
        }

        [Fact]
        public async Task GetGame_WithValidId_ReturnsOkResult_WithGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Name = "FizzBuzz",
                Author = "Test Author",
                DateCreated = DateTime.UtcNow,
                Rules = new List<GameRule>
                {
                    new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                    new GameRule { Id = Guid.NewGuid(), Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            var result = await _controller.GetGame(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGame = Assert.IsType<GameDto>(okResult.Value);
            Assert.Equal(game.Id, returnedGame.Id);
            Assert.Equal(game.Name, returnedGame.Name);
            Assert.Equal(2, returnedGame.Rules.Count);
        }

        [Fact]
        public async Task GetGame_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.GetGame(gameId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateGame_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createGameDto = new CreateGameDto
            {
                Name = "FizzBuzz",
                Author = "Test Author",
                Rules = new List<CreateGameRuleDto>
                {
                    new CreateGameRuleDto { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                    new CreateGameRuleDto { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetGameByNameAsync(createGameDto.Name))
                .ReturnsAsync((Game)null);

            _mockGameRepository.Setup(repo => repo.CreateGameAsync(It.IsAny<Game>()))
                .ReturnsAsync((Game game) => game);

            // Act
            var result = await _controller.CreateGame(createGameDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedGame = Assert.IsType<GameDto>(createdAtActionResult.Value);
            Assert.Equal(createGameDto.Name, returnedGame.Name);
            Assert.Equal(createGameDto.Author, returnedGame.Author);
            Assert.Equal(2, returnedGame.Rules.Count);
            Assert.Equal("GetGame", createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task CreateGame_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var createGameDto = new CreateGameDto
            {
                Name = "FizzBuzz",
                Author = "Test Author",
                Rules = new List<CreateGameRuleDto>
                {
                    new CreateGameRuleDto { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                }
            };

            var existingGame = new Game
            {
                Id = Guid.NewGuid(),
                Name = "FizzBuzz",
                Author = "Existing Author"
            };

            _mockGameRepository.Setup(repo => repo.GetGameByNameAsync(createGameDto.Name))
                .ReturnsAsync(existingGame);

            // Act
            var result = await _controller.CreateGame(createGameDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already exists", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateGame_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var updateGameDto = new CreateGameDto
            {
                Name = "FizzBuzz Updated",
                Author = "Test Author Updated",
                Rules = new List<CreateGameRuleDto>
                {
                    new CreateGameRuleDto { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                    new CreateGameRuleDto { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                }
            };

            var existingGame = new Game
            {
                Id = gameId,
                Name = "FizzBuzz",
                Author = "Test Author",
                Rules = new List<GameRule>
                {
                    new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(existingGame);

            _mockGameRepository.Setup(repo => repo.GetGameByNameAsync(updateGameDto.Name))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.UpdateGame(gameId, updateGameDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the game was updated
            _mockGameRepository.Verify(repo => repo.UpdateGameAsync(It.Is<Game>(g =>
                g.Id == gameId &&
                g.Name == updateGameDto.Name &&
                g.Author == updateGameDto.Author &&
                g.Rules.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task UpdateGame_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var updateGameDto = new CreateGameDto
            {
                Name = "FizzBuzz Updated",
                Author = "Test Author Updated",
                Rules = new List<CreateGameRuleDto>
                {
                    new CreateGameRuleDto { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.UpdateGame(gameId, updateGameDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteGame_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var existingGame = new Game
            {
                Id = gameId,
                Name = "FizzBuzz",
                Author = "Test Author"
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(existingGame);

            // Act
            var result = await _controller.DeleteGame(gameId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the game was deleted
            _mockGameRepository.Verify(repo => repo.DeleteGameAsync(gameId), Times.Once);
        }

        [Fact]
        public async Task DeleteGame_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.DeleteGame(gameId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
