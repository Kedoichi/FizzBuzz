using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FizzBuzzGame.API.DTOs
{
    public class CreateSessionDto
    {
        [Required(ErrorMessage = "Game ID is required")]
        public Guid GameId { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(10, 300, ErrorMessage = "Duration must be between 10 and 300 seconds")]
        public int Duration { get; set; } // In seconds
    }

    public class SessionDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int RemainingTime { get; set; } // In seconds
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public List<GameRuleDto> Rules { get; set; } = new();
        public string ssID { get; set; }
    }

    public class GameRoundDto
    {
        public int Number { get; set; }
    }
    public class AnswerDto
    {
        [Required(ErrorMessage = "Answer is required")]
        public string Answer { get; set; } = string.Empty;
    }

    public class AnswerResultDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsGameOver { get; set; }
    }

    public class GameResultDto
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int AccuracyPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }
}
