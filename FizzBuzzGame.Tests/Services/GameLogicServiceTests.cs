using System;
using System.Collections.Generic;
using Xunit;
using FizzBuzzGame.Core.Models;
using FizzBuzzGame.Core.Services;

namespace FizzBuzzGame.Tests.Services
{
    public class GameLogicServiceTests
    {
        private readonly GameLogicService _gameLogicService;

        public GameLogicServiceTests()
        {
            _gameLogicService = new GameLogicService();
        }

        [Fact]
        public void CalculateAnswer_WithNoMatchingRules_ReturnsOriginalNumber()
        {
            // Arrange
            var rules = new List<GameRule>
            {
                new GameRule { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                new GameRule { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
            };

            // Act
            var result = _gameLogicService.CalculateAnswer(1, rules);

            // Assert
            Assert.Equal("1", result);
        }

        [Fact]
        public void CalculateAnswer_WithOneMatchingRule_ReturnsReplaceWord()
        {
            // Arrange
            var rules = new List<GameRule>
            {
                new GameRule { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                new GameRule { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
            };

            // Act
            var result = _gameLogicService.CalculateAnswer(3, rules);

            // Assert
            Assert.Equal("Fizz", result);
        }

        [Fact]
        public void CalculateAnswer_WithMultipleMatchingRules_ReturnsCombinedWords()
        {
            // Arrange
            var rules = new List<GameRule>
            {
                new GameRule { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 },
                new GameRule { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 1 }
            };

            // Act
            var result = _gameLogicService.CalculateAnswer(15, rules);

            // Assert
            Assert.Equal("FizzBuzz", result);
        }

        [Fact]
        public void CalculateAnswer_RespectsRuleSortOrder()
        {
            // Arrange
            var rules = new List<GameRule>
            {
                new GameRule { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 1 },
                new GameRule { Divisor = 5, ReplaceWord = "Buzz", SortOrder = 0 }
            };

            // Act
            var result = _gameLogicService.CalculateAnswer(15, rules);

            // Assert
            Assert.Equal("BuzzFizz", result); // Sort order is reversed, so Buzz comes first
        }

        [Fact]
        public void CalculateAnswer_WithNegativeNumber_ThrowsArgumentException()
        {
            // Arrange
            var rules = new List<GameRule>
            {
                new GameRule { Divisor = 3, ReplaceWord = "Fizz", SortOrder = 0 }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _gameLogicService.CalculateAnswer(-1, rules));
        }

        [Fact]
        public void GenerateRandomNumber_ReturnsNumberInRange()
        {
            // Arrange
            int min = 1;
            int max = 100;
            var usedNumbers = new List<int>();

            // Act
            var result = _gameLogicService.GenerateRandomNumber(min, max, usedNumbers);

            // Assert
            Assert.InRange(result, min, max);
        }

        [Fact]
        public void GenerateRandomNumber_ExcludesUsedNumbers()
        {
            // Arrange
            int min = 1;
            int max = 10;
            var usedNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Act
            var result = _gameLogicService.GenerateRandomNumber(min, max, usedNumbers);

            // Assert
            Assert.Equal(10, result); // Only 10 is left
        }

        [Fact]
        public void GenerateRandomNumber_WhenAllNumbersUsed_ThrowsInvalidOperationException()
        {
            // Arrange
            int min = 1;
            int max = 3;
            var usedNumbers = new List<int> { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _gameLogicService.GenerateRandomNumber(min, max, usedNumbers));
        }

        [Fact]
        public void ValidateAnswer_WithCorrectAnswer_ReturnsTrue()
        {
            // Arrange
            string expectedAnswer = "FizzBuzz";
            string playerAnswer = "FizzBuzz";

            // Act
            var result = _gameLogicService.ValidateAnswer(expectedAnswer, playerAnswer);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateAnswer_WithIncorrectAnswer_ReturnsFalse()
        {
            // Arrange
            string expectedAnswer = "FizzBuzz";
            string playerAnswer = "Fizz";

            // Act
            var result = _gameLogicService.ValidateAnswer(expectedAnswer, playerAnswer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateAnswer_IgnoresCase()
        {
            // Arrange
            string expectedAnswer = "FizzBuzz";
            string playerAnswer = "fizzbuzz";

            // Act
            var result = _gameLogicService.ValidateAnswer(expectedAnswer, playerAnswer);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateAnswer_IgnoresExtraWhitespace()
        {
            // Arrange
            string expectedAnswer = "FizzBuzz";
            string playerAnswer = "   FizzBuzz  ";

            // Act
            var result = _gameLogicService.ValidateAnswer(expectedAnswer, playerAnswer);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CalculateAccuracy_WithZeroAnswers_ReturnsZero()
        {
            // Act
            var result = _gameLogicService.CalculateAccuracy(0, 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateAccuracy_WithAllCorrect_Returns100Percent()
        {
            // Act
            var result = _gameLogicService.CalculateAccuracy(10, 10);

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void CalculateAccuracy_WithHalfCorrect_Returns50Percent()
        {
            // Act
            var result = _gameLogicService.CalculateAccuracy(5, 10);

            // Assert
            Assert.Equal(50, result);
        }
    }
}
