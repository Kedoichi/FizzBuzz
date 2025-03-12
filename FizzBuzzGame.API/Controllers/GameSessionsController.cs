using System;
using System.Linq;
using System.Threading.Tasks;
using FizzBuzzGame.API.DTOs;
using FizzBuzzGame.Core.Interfaces;
using FizzBuzzGame.Core.Models;
using FizzBuzzGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FizzBuzzGame.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameSessionsController : ControllerBase
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameSessionRepository _sessionRepository;
        private readonly GameLogicService _gameLogicService;
        private readonly ILogger<GameSessionsController> _logger;

        private const int MIN_NUMBER = 1;
        private const int MAX_NUMBER = 1000;

        public GameSessionsController(
            IGameRepository gameRepository,
            IGameSessionRepository sessionRepository,
            GameLogicService gameLogicService,
            ILogger<GameSessionsController> logger)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _gameLogicService = gameLogicService;
            _logger = logger;
        }

        /// <summary>
        /// Starts a new game session
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SessionDto>> StartSession(CreateSessionDto createSessionDto)
        {
            try
            {
                // Validate input
                if (createSessionDto.Duration <= 0)
                {
                    return BadRequest("Game duration must be positive.");
                }

                // Check if the game exists
                var game = await _gameRepository.GetGameByIdAsync(createSessionDto.GameId);
                if (game == null)
                {
                    _logger.LogWarning("Game with ID {GameId} not found", createSessionDto.GameId);
                    return NotFound($"Game with ID {createSessionDto.GameId} not found.");
                }

                // Create a new session
                var session = new GameSession
                {
                    GameId = game.Id,
                    ssID= Guid.NewGuid().ToString(),
                    PlayerId = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous", // Use IP or anonymous
                    Duration = createSessionDto.Duration,
                    StartTime = DateTime.UtcNow,
                    Game = game
                };

                await _sessionRepository.CreateSessionAsync(session);

                _logger.LogInformation("Game session started: {SessionId} for game {GameName}",
                    session.Id, game.Name);

                return Ok(new SessionDto
                {
                    Id = session.Id,
                    GameId = session.GameId,
                    ssID = session.ssID,
                    GameName = game.Name,
                    StartTime = session.StartTime,
                    Duration = session.Duration,

                    RemainingTime = session.Duration,
                    CorrectAnswers = 0,
                    IncorrectAnswers = 0,
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
                _logger.LogError(ex, "Error starting game session for game ID {GameId}", createSessionDto.GameId);
                return StatusCode(500, "An error occurred while starting the game session.");
            }
        }

        /// <summary>
        /// Gets information about a specific game session
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SessionDto>> GetSession(Guid id)
        {
            try
            {
                var session = await _sessionRepository.GetSessionByIdAsync(id);

                if (session == null || session.Game == null)
                {
                    _logger.LogWarning("Session with ID {SessionId} not found", id);
                    return NotFound();
                }

                // Calculate remaining time
                int remainingTime = 0;
                if (session.EndTime == null)
                {
                    var elapsedTime = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                    remainingTime = Math.Max(0, session.Duration - elapsedTime);
                }

                return Ok(new SessionDto
                {
                    Id = session.Id,
                    GameId = session.GameId,
                    GameName = session.Game.Name,
                    StartTime = session.StartTime,
                    Duration = session.Duration,
                    RemainingTime = remainingTime,
                    CorrectAnswers = session.CorrectAnswers,
                    IncorrectAnswers = session.IncorrectAnswers,
                    Rules = session.Game.Rules.Select(rule => new GameRuleDto
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
                _logger.LogError(ex, "Error retrieving session with ID {SessionId}", id);
                return StatusCode(500, $"An error occurred while retrieving session with ID {id}.");
            }
        }

        /// <summary>
        /// Gets the next random number for a game session
        /// </summary>
        [HttpGet("{id}/next")]
        public async Task<ActionResult<GameRoundDto>> GetNextNumber(Guid id)
        {
            try
            {
                var session = await _sessionRepository.GetSessionByIdAsync(id);

                if (session == null || session.Game == null)
                {
                    _logger.LogWarning("Session with ID {SessionId} not found", id);
                    return NotFound();
                }

                // Check if the game is already over
                if (session.EndTime != null)
                {
                    return BadRequest("This game session is already completed.");
                }

                // Check if the time is up
                var elapsedTime = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                if (elapsedTime >= session.Duration)
                {
                    // End the session if time is up
                    await _sessionRepository.EndSessionAsync(id);
                    return BadRequest("Time's up for this game session.");
                }

                try
                {
                    // Generate a random number that hasn't been used yet
                    int number = _gameLogicService.GenerateRandomNumber(MIN_NUMBER, MAX_NUMBER, session.UsedNumbers);

                    // Add the number to the used numbers list
                    session.UsedNumbers.Add(number);
                    await _sessionRepository.UpdateSessionAsync(session);

                    _logger.LogInformation("Generated number {Number} for session {SessionId}",
                        number, session.Id);

                    return Ok(new GameRoundDto { Number = number });
                }
                catch (InvalidOperationException ex)
                {
                    // This happens if all numbers have been used
                    _logger.LogWarning(ex, "All available numbers used for session {SessionId}", id);
                    return BadRequest(ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating next number for session {SessionId}", id);
                return StatusCode(500, $"An error occurred while generating the next number.");
            }
        }

        /// <summary>
        /// Submits a player's answer for validation
        /// </summary>
        [HttpPost("{id}/answer")]
        public async Task<ActionResult<AnswerResultDto>> SubmitAnswer(Guid id, AnswerDto answerDto)
        {
            try
            {
                var session = await _sessionRepository.GetSessionByIdAsync(id);

                if (session == null || session.Game == null)
                {
                    _logger.LogWarning("Session with ID {SessionId} not found", id);
                    return NotFound();
                }

                // Check if the game is already over
                if (session.EndTime != null)
                {
                    return BadRequest("This game session is already completed.");
                }

                // Check if the time is up
                var elapsedTime = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                bool isGameOver = elapsedTime >= session.Duration;

                if (isGameOver)
                {
                    // End the session if time is up
                    await _sessionRepository.EndSessionAsync(id);
                    return BadRequest("Time's up for this game session.");
                }

                // Get the last number from the session
                if (!session.UsedNumbers.Any())
                {
                    return BadRequest("No numbers have been generated for this session yet.");
                }

                int lastNumber = session.UsedNumbers.Last();
                string expectedAnswer = _gameLogicService.CalculateAnswer(lastNumber, session.Game.Rules);

                // Validate the answer
                bool isCorrect = _gameLogicService.ValidateAnswer(expectedAnswer, answerDto.Answer);

                // Create a new round
                var round = new GameRound
                {
                    GameSessionId = session.Id,
                    Number = lastNumber,
                    ExpectedAnswer = expectedAnswer,
                    PlayerAnswer = answerDto.Answer,
                    IsCorrect = isCorrect,
                    ResponseTimeMs = 0  // Ideally we'd calculate this from the client
                };

                // Add the round to the session
                await _sessionRepository.AddRoundToSessionAsync(round);

                _logger.LogInformation(
                    "Answer submitted for session {SessionId}: Number={Number}, Expected={Expected}, Player={Player}, Correct={Correct}",
                    session.Id, lastNumber, expectedAnswer, answerDto.Answer, isCorrect);

                return Ok(new AnswerResultDto
                {
                    IsCorrect = isCorrect,
                    CorrectAnswer = expectedAnswer,
                    IsGameOver = isGameOver
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer for session {SessionId}", id);
                return StatusCode(500, "An error occurred while submitting your answer.");
            }
        }

        /// <summary>
        /// Gets the final result of a game session
        /// </summary>
        [HttpGet("{id}/result")]
        public async Task<ActionResult<GameResultDto>> GetGameResult(Guid id)
        {
            try
            {
                var session = await _sessionRepository.GetSessionByIdAsync(id);

                if (session == null || session.Game == null)
                {
                    _logger.LogWarning("Session with ID {SessionId} not found", id);
                    return NotFound();
                }

                // If the session hasn't ended yet but the time is up, end it
                if (session.EndTime == null)
                {
                    var elapsedTime = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                    if (elapsedTime >= session.Duration)
                    {
                        await _sessionRepository.EndSessionAsync(id);

                        // Refresh the session data
                        session = await _sessionRepository.GetSessionByIdAsync(id);
                    }
                }

                // Calculate totals
                int totalAnswers = session.CorrectAnswers + session.IncorrectAnswers;
                int accuracyPercentage = _gameLogicService.CalculateAccuracy(session.CorrectAnswers, totalAnswers);

                return Ok(new GameResultDto
                {
                    Id = session.Id,
                    GameId = session.GameId,
                    GameName = session.Game.Name,
                    TotalAnswers = totalAnswers,
                    CorrectAnswers = session.CorrectAnswers,
                    IncorrectAnswers = session.IncorrectAnswers,
                    AccuracyPercentage = accuracyPercentage,
                    IsCompleted = session.EndTime != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game result for session {SessionId}", id);
                return StatusCode(500, "An error occurred while retrieving the game result.");
            }
        }
    }
}
