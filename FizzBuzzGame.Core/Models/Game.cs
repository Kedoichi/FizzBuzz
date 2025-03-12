using System;
using System.Collections.Generic;

namespace FizzBuzzGame.Core.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<GameRule> Rules { get; set; } = new List<GameRule>();
    }
}
