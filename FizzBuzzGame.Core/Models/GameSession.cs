using System;
using System.Collections.Generic;

namespace FizzBuzzGame.Core.Models
{
    public class GameSession
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; } // In seconds
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public List<int> UsedNumbers { get; set; } = new List<int>();
        public string ssID { get; set; }

        // Navigation property
        public Game? Game { get; set; }
        public List<GameRound> Rounds { get; set; } = new List<GameRound>();
    }
}
