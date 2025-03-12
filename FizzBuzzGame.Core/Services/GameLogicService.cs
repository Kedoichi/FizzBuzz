// FizzBuzzGame.Core/Services/GameLogicService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using FizzBuzzGame.Core.Models;

namespace FizzBuzzGame.Core.Services
{
    public class GameLogicService
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Calculates the expected answer for a given number based on game rules
        /// </summary>
        public string CalculateAnswer(int number, IEnumerable<GameRule> rules)
        {
            if (number < 1)
            {
                throw new ArgumentException("Number must be positive", nameof(number));
            }

            string result = string.Empty;

            // Process rules in order of their SortOrder
            foreach (var rule in rules.OrderBy(r => r.SortOrder))
            {
                if (number % rule.Divisor == 0)
                {
                    result += rule.ReplaceWord;
                }
            }

            // If no rules matched, return the original number as string
            return string.IsNullOrEmpty(result) ? number.ToString() : result;
        }

        /// <summary>
        /// Generates a random number that hasn't been used in the current game session
        /// </summary>
        public int GenerateRandomNumber(int min, int max, IEnumerable<int> usedNumbers)
        {
            if (min < 1)
            {
                throw new ArgumentException("Minimum number must be positive", nameof(min));
            }

            if (max <= min)
            {
                throw new ArgumentException("Maximum number must be greater than minimum", nameof(max));
            }

            // Create a list of available numbers
            var availableNumbers = Enumerable.Range(min, max - min + 1)
                .Except(usedNumbers)
                .ToList();

            // If all numbers have been used, throw an exception
            if (availableNumbers.Count == 0)
            {
                throw new InvalidOperationException($"All numbers between {min} and {max} have been used.");
            }

            // Pick a random available number
            int index = _random.Next(0, availableNumbers.Count);
            return availableNumbers[index];
        }

        /// <summary>
        /// Validates if the player's answer matches the expected answer
        /// </summary>
        public bool ValidateAnswer(string expectedAnswer, string playerAnswer)
        {
            if (string.IsNullOrWhiteSpace(playerAnswer))
            {
                return false;
            }

            return string.Equals(expectedAnswer.Trim(), playerAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the accuracy percentage
        /// </summary>
        public int CalculateAccuracy(int correctAnswers, int totalAnswers)
        {
            if (totalAnswers == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)correctAnswers / totalAnswers * 100);
        }
    }
}
