using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FizzBuzzGame.API.Controllers;
using FizzBuzzGame.API.DTOs;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using FizzBuzzGame.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FizzBuzzGame.Tests.Controllers
{
    public class GameSessionsControllerTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IGameSessionRepository> _mockSessionRepository;
        private readonly GameLogicService _gameLogicService;
        private readonly Mock<ILogger<GameSessionsController>> _mockLogger;
        private readonly GameSessionsController _controller;

        public GameSessionsControllerTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockSessionRepository = new Mock<IGameSessionRepository>();
            _gameLogicService = new GameLogicService();
            _mockLogger = new Mock<ILogger<GameSessionsController>>();

            _controller = new GameSessionsController(
                _mockGameRepository.Object,
                _mockSessionRepository.Object,
                _gameLogicService,
                _mockLogger.Object);

            // Setup default HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task StartSession_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var createSessionDto = new CreateSessionDto
            {
                GameId = gameId,
                Duration = 60
            };

            var game = new Game
            {
                Id = gameId,
                Name = "FizzBuzz",
                Author = "Test Author",
                Rules = new List<GameRule>
                {
                    new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                    new GameRule { Id = Guid.NewGuid(), Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                }
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync(game);

            _mockSessionRepository.Setup(repo => repo.CreateSessionAsync(It.IsAny<GameSession>()))
                .ReturnsAsync((GameSession session) =>
                {
                    session.Id = Guid.NewGuid();
                    return session;
                });

            // Act
            var result = await _controller.StartSession(createSessionDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sessionDto = Assert.IsType<SessionDto>(okResult.Value);
            Assert.Equal(gameId, sessionDto.GameId);
            Assert.Equal(game.Name, sessionDto.GameName);
            Assert.Equal(createSessionDto.Duration, sessionDto.Duration);
            Assert.Equal(createSessionDto.Duration, sessionDto.RemainingTime);
            Assert.Equal(0, sessionDto.CorrectAnswers);
            Assert.Equal(0, sessionDto.IncorrectAnswers);
            Assert.Equal(2, sessionDto.Rules.Count);
        }

        [Fact]
        public async Task StartSession_WithInvalidGameId_ReturnsNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var createSessionDto = new CreateSessionDto
            {
                GameId = gameId,
                Duration = 60
            };

            _mockGameRepository.Setup(repo => repo.GetGameByIdAsync(gameId))
                .ReturnsAsync((Game)null);

            // Act
            var result = await _controller.StartSession(createSessionDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task StartSession_WithInvalidDuration_ReturnsBadRequest()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var createSessionDto = new CreateSessionDto
            {
                GameId = gameId,
                Duration = 0 // Invalid duration
            };

            // Act
            var result = await _controller.StartSession(createSessionDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSession_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-10); // Started 10 seconds ago

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                CorrectAnswers = 2,
                IncorrectAnswers = 1,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                        new GameRule { Id = Guid.NewGuid(), Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
                    }
                }
            };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var result = await _controller.GetSession(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sessionDto = Assert.IsType<SessionDto>(okResult.Value);
            Assert.Equal(sessionId, sessionDto.Id);
            Assert.Equal(gameId, sessionDto.GameId);
            Assert.Equal("FizzBuzz", sessionDto.GameName);
            Assert.Equal(startTime, sessionDto.StartTime);
            Assert.Equal(60, sessionDto.Duration);
            Assert.InRange(sessionDto.RemainingTime, 49, 51); // About 50 seconds left (60 - 10)
            Assert.Equal(2, sessionDto.CorrectAnswers);
            Assert.Equal(1, sessionDto.IncorrectAnswers);
            Assert.Equal(2, sessionDto.Rules.Count);
        }

        [Fact]
        public async Task GetSession_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync((GameSession)null);

            // Act
            var result = await _controller.GetSession(sessionId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetNextNumber_WithValidSession_ReturnsOkResult()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-10); // Started 10 seconds ago

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                UsedNumbers = new List<int>(),
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                    }
                }
            };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            _mockSessionRepository.Setup(repo => repo.UpdateSessionAsync(It.IsAny<GameSession>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.GetNextNumber(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var roundDto = Assert.IsType<GameRoundDto>(okResult.Value);
            Assert.InRange(roundDto.Number, 1, 1000); // Number should be in allowed range

            // Verify the session was updated with the used number
            _mockSessionRepository.Verify(repo => repo.UpdateSessionAsync(It.Is<GameSession>(s =>
                s.Id == sessionId &&
                s.UsedNumbers.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task GetNextNumber_WithTimeUp_ReturnsBadRequest()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-70); // Started 70 seconds ago (exceeds 60 second duration)

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>()
                }
            };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            _mockSessionRepository.Setup(repo => repo.EndSessionAsync(sessionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.GetNextNumber(sessionId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Time's up", badRequestResult.Value.ToString());

            // Verify the session was ended
            _mockSessionRepository.Verify(repo => repo.EndSessionAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task GetNextNumber_WithCompletedSession_ReturnsBadRequest()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-30);
            var endTime = DateTime.UtcNow.AddSeconds(-10);

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                EndTime = endTime, // Session is already completed
                Duration = 60,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>()
                }
            };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var result = await _controller.GetNextNumber(sessionId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already completed", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task SubmitAnswer_WithValidAnswer_ReturnsOkResult()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-10);

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                UsedNumbers = new List<int> { 3 }, // Last number was 3
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                    }
                }
            };

            var answerDto = new AnswerDto { Answer = "Fizz" }; // Correct answer for 3

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            _mockSessionRepository.Setup(repo => repo.AddRoundToSessionAsync(It.IsAny<GameRound>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubmitAnswer(sessionId, answerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var answerResultDto = Assert.IsType<AnswerResultDto>(okResult.Value);
            Assert.True(answerResultDto.IsCorrect);
            Assert.Equal("Fizz", answerResultDto.CorrectAnswer);
            Assert.False(answerResultDto.IsGameOver);

            // Verify the round was added
            _mockSessionRepository.Verify(repo => repo.AddRoundToSessionAsync(It.Is<GameRound>(r =>
                r.GameSessionId == sessionId &&
                r.Number == 3 &&
                r.ExpectedAnswer == "Fizz" &&
                r.PlayerAnswer == "Fizz" &&
                r.IsCorrect)), Times.Once);
        }

        [Fact]
        public async Task SubmitAnswer_WithInvalidAnswer_ReturnsOkResultWithCorrection()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-10);

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                UsedNumbers = new List<int> { 3 }, // Last number was 3
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                    }
                }
            };

            var answerDto = new AnswerDto { Answer = "3" }; // Incorrect answer for 3

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            _mockSessionRepository.Setup(repo => repo.AddRoundToSessionAsync(It.IsAny<GameRound>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubmitAnswer(sessionId, answerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var answerResultDto = Assert.IsType<AnswerResultDto>(okResult.Value);
            Assert.False(answerResultDto.IsCorrect);
            Assert.Equal("Fizz", answerResultDto.CorrectAnswer);
            Assert.False(answerResultDto.IsGameOver);

            // Verify the round was added
            _mockSessionRepository.Verify(repo => repo.AddRoundToSessionAsync(It.Is<GameRound>(r =>
                r.GameSessionId == sessionId &&
                r.Number == 3 &&
                r.ExpectedAnswer == "Fizz" &&
                r.PlayerAnswer == "3" &&
                !r.IsCorrect)), Times.Once);
        }

        [Fact]
        public async Task SubmitAnswer_WithTimeUp_ReturnsBadRequest()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-70); // Started 70 seconds ago (exceeds 60 second duration)

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                Duration = 60,
                UsedNumbers = new List<int> { 3 },
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>
                    {
                        new GameRule { Id = Guid.NewGuid(), Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
                    }
                }
            };

            var answerDto = new AnswerDto { Answer = "Fizz" };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            _mockSessionRepository.Setup(repo => repo.EndSessionAsync(sessionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubmitAnswer(sessionId, answerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Time's up", badRequestResult.Value.ToString());

            // Verify the session was ended
            _mockSessionRepository.Verify(repo => repo.EndSessionAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task GetGameResult_WithCompletedSession_ReturnsOkResult()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddMinutes(-2);
            var endTime = DateTime.UtcNow.AddMinutes(-1);

            var session = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                EndTime = endTime,
                Duration = 60,
                CorrectAnswers = 8,
                IncorrectAnswers = 2,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>()
                }
            };

            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var result = await _controller.GetGameResult(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultDto = Assert.IsType<GameResultDto>(okResult.Value);
            Assert.Equal(sessionId, resultDto.Id);
            Assert.Equal("FizzBuzz", resultDto.GameName);
            Assert.Equal(10, resultDto.TotalAnswers);
            Assert.Equal(8, resultDto.CorrectAnswers);
            Assert.Equal(2, resultDto.IncorrectAnswers);
            Assert.Equal(80, resultDto.AccuracyPercentage);
            Assert.True(resultDto.IsCompleted);
        }

        [Fact]
        public async Task GetGameResult_WithActiveSessionButTimeUp_EndsSessionAndReturnsResult()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-70); // Started 70 seconds ago (exceeds 60 second duration)

            var initialSession = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                EndTime = null, // Session not ended yet
                Duration = 60,
                CorrectAnswers = 5,
                IncorrectAnswers = 5,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>()
                }
            };

            var endedSession = new GameSession
            {
                Id = sessionId,
                GameId = gameId,
                PlayerId = "127.0.0.1",
                StartTime = startTime,
                EndTime = DateTime.UtcNow, // Now ended
                Duration = 60,
                CorrectAnswers = 5,
                IncorrectAnswers = 5,
                Game = new Game
                {
                    Id = gameId,
                    Name = "FizzBuzz",
                    Rules = new List<GameRule>()
                }
            };

            // Track if EndSessionAsync has been called
            bool endSessionHasBeenCalled = false;

            // Setup to first return the initial session, then return the ended session after EndSessionAsync is called
            _mockSessionRepository.Setup(repo => repo.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(() => endSessionHasBeenCalled ? endedSession : initialSession);

            _mockSessionRepository.Setup(repo => repo.EndSessionAsync(sessionId))
                .Returns(Task.CompletedTask)
                .Callback(() => endSessionHasBeenCalled = true);

            // Act
            var result = await _controller.GetGameResult(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultDto = Assert.IsType<GameResultDto>(okResult.Value);
            Assert.Equal(sessionId, resultDto.Id);
            Assert.Equal("FizzBuzz", resultDto.GameName);
            Assert.Equal(10, resultDto.TotalAnswers);
            Assert.Equal(5, resultDto.CorrectAnswers);
            Assert.Equal(5, resultDto.IncorrectAnswers);
            Assert.Equal(50, resultDto.AccuracyPercentage);
            Assert.True(resultDto.IsCompleted);

            // Verify the session was ended
            _mockSessionRepository.Verify(repo => repo.EndSessionAsync(sessionId), Times.Once);
        }
    }
}
