using System;

namespace FizzBuzzGame.Core.Models
{
    public class GameRule
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public int Divisor { get; set; }
        public string ReplaceWord { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        // Navigation property
        public Game? Game { get; set; }
    }
}
