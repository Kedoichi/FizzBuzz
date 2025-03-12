using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FizzBuzzGame.API.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public List<GameRuleDto> Rules { get; set; } = new();
    }

    public class CreateGameDto
    {
        [Required(ErrorMessage = "Game name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Game name must be between 3 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 50 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one rule is required")]
        [MinLength(1, ErrorMessage = "At least one rule is required")]
        public List<CreateGameRuleDto> Rules { get; set; } = new();
    }
}
