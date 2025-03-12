using System;
using System.ComponentModel.DataAnnotations;

namespace FizzBuzzGame.API.DTOs
{
    public class GameRuleDto
    {
        public Guid Id { get; set; }
        public int Divisor { get; set; }
        public string ReplaceWord { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class CreateGameRuleDto
    {
        [Required(ErrorMessage = "Divisor is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Divisor must be positive")]
        public int Divisor { get; set; }

        [Required(ErrorMessage = "Replace word is required")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Replace word must be between 1 and 20 characters")]
        public string ReplaceWord { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be non-negative")]
        public int SortOrder { get; set; }
    }
}
