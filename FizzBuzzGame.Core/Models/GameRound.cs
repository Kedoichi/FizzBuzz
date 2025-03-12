using System;

namespace FizzBuzzGame.Core.Models
{
    public class GameRound
    {
        public Guid Id { get; set; }
        public Guid GameSessionId { get; set; }
        public int Number { get; set; }
        public string ExpectedAnswer { get; set; } = string.Empty;
        public string? PlayerAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; }

        // Navigation property
        public GameSession? GameSession { get; set; }
    }
}
