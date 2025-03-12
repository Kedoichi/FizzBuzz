"use client";

import { useState, useEffect, useRef } from "react";
import { useParams, useRouter } from "next/navigation";
import type {
  Game,
  GameSession,
  AnswerResult,
  CreateSession,
} from "../../../types";
import { gameService, sessionService } from "../../../services/apiService";
import { formatTime } from "../../../utils/formatters";
import Card from "../../../components/Card";
import Button from "../../../components/Button";
import TextInput from "../../../components/TextInput";
import LoadingSpinner from "../../../components/LoadingSpinner";
import Badge from "../../../components/Badge";

// Game states
enum GameState {
  LOADING,
  SETUP,
  PLAYING,
  WAITING_FOR_ANSWER,
  FEEDBACK,
  GAME_OVER,
  ERROR,
}

export default function GamePlayPage() {
  const router = useRouter();
  const params = useParams();
  const gameId = params.gameId as string;

  // Game state and data
  const [gameState, setGameState] = useState<GameState>(GameState.LOADING);
  const [game, setGame] = useState<Game | null>(null);
  const [session, setSession] = useState<GameSession | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Game play state
  const [duration, setDuration] = useState<number>(60);
  const [currentNumber, setCurrentNumber] = useState<number | null>(null);
  const [answer, setAnswer] = useState<string>("");
  const [remainingTime, setRemainingTime] = useState<number>(0);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);

  // Refs for timers
  const timerRef = useRef<number | null>(null);
  const sessionCheckRef = useRef<number | null>(null);
  const answerInputRef = useRef<HTMLInputElement>(null);

  // Load the game
  useEffect(() => {
    const loadGame = async () => {
      if (!gameId) return;

      try {
        setGameState(GameState.LOADING);
        const gameData = await gameService.getGame(gameId);
        setGame(gameData);
        setGameState(GameState.SETUP);
      } catch (err) {
        console.error("Error loading game:", err);
        setError("Failed to load game. Please try again.");
        setGameState(GameState.ERROR);
      }
    };

    loadGame();

    // Cleanup
    return () => {
      if (timerRef.current) window.clearInterval(timerRef.current);
      if (sessionCheckRef.current)
        window.clearInterval(sessionCheckRef.current);
    };
  }, [gameId]);

  // Start a new game session
  const startGame = async () => {
    if (!gameId) return;

    try {
      // Reset all game-related states
      setGameState(GameState.LOADING);
      setSession(null);
      setCurrentNumber(null);
      setAnswer("");
      setAnswerResult(null);
      setRemainingTime(0);

      // Clear any existing timers
      if (timerRef.current) {
        window.clearInterval(timerRef.current);
        timerRef.current = null;
      }
      if (sessionCheckRef.current) {
        window.clearInterval(sessionCheckRef.current);
        sessionCheckRef.current = null;
      }

      const createSessionData: CreateSession = {
        gameId,
        duration,
      };

      const sessionData = await sessionService.startSession(createSessionData);

      setSession(sessionData);

      setRemainingTime(sessionData.duration);

      // Start the timer
      startTimer(sessionData.id);

      // Get the first number
      await getNextNumber(sessionData.id);

      setGameState(GameState.PLAYING);

      // Focus on the answer input
      if (answerInputRef.current) {
        answerInputRef.current.focus();
      }
    } catch (err) {
      console.error("Error starting game:", err);
      setError("Failed to start game. Please try again.");
      setGameState(GameState.ERROR);
    }
  };
  // Start the countdown timer
  const startTimer = (id: string) => {
    if (timerRef.current) window.clearInterval(timerRef.current);

    timerRef.current = window.setInterval(() => {
      setRemainingTime((prev) => {
        const newTime = prev - 1;
        if (newTime <= 0) {
          // Time's up!
          endGame(id);

          window.clearInterval(timerRef.current!);
          return 0;
        }
        return newTime;
      });
    }, 1000);
  };

  // Get the next number from the server
  const getNextNumber = async (sessionId: string) => {
    try {
      const roundData = await sessionService.getNextNumber(sessionId);
      setCurrentNumber(roundData.number);
      setAnswer("");
      setAnswerResult(null);
    } catch (err) {
      console.error("Error getting next number:", err);

      // Check if the game is over (time's up)
      if (session) {
        const sessionData = await sessionService.getSession(session.id);
        if (sessionData.remainingTime <= 0) {
          endGame(session.id);
        }
      }
    }
  };

  // Submit the player's answer
  const submitAnswer = async () => {
    if (!session || !answer.trim()) return;

    try {
      setGameState(GameState.WAITING_FOR_ANSWER);

      const result = await sessionService.submitAnswer(session.id, {
        answer: answer.trim(),
      });
      setAnswerResult(result);

      // Update session data
      const sessionData = await sessionService.getSession(session.id);
      setSession(sessionData);

      if (result.isGameOver) {
        endGame(session.id);
      } else {
        setGameState(GameState.FEEDBACK);
      }
    } catch (err) {
      console.error("Error submitting answer:", err);

      // Check if the game is over
      if (session) {
        try {
          const sessionData = await sessionService.getSession(session.id);
          setSession(sessionData);

          if (sessionData.remainingTime <= 0) {
            endGame(session.id);
          } else {
            setGameState(GameState.PLAYING);
          }
        } catch (error) {
          setError("Failed to check game status. Please try again." + error);
          setGameState(GameState.ERROR);
        }
      }
    }
  };

  // Get next number after answering
  const continueGame = async () => {
    if (!session) return;

    if (answerResult?.isGameOver) {
      endGame(session.id);
    } else {
      await getNextNumber(session.id);
      setGameState(GameState.PLAYING);

      // Focus on the answer input
      if (answerInputRef.current) {
        answerInputRef.current.focus();
      }
    }
  };

  // End the game and show results
  const endGame = async (id: string | undefined) => {
    try {
      // Stop the timer
      if (timerRef.current) {
        window.clearInterval(timerRef.current);
        timerRef.current = null;
      }
      setGameState(GameState.GAME_OVER);

      router.push(`/results/${id}?timeout=true`);
    } catch (err) {
      console.error("Error ending game:", err);
      setError("Failed to end the game. Please try again.");
      setGameState(GameState.ERROR);
    }
  };

  // Handle form submission
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    submitAnswer();
  };

  // Handle enter key in setup phase to start game
  const handleSetupKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      startGame();
    }
  };

  // The game setup UI
  if (gameState === GameState.SETUP && game) {
    return (
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold mb-2">{game.name}</h1>
        <p className="text-gray-600 mb-6">Created by {game.author}</p>

        <Card className="mb-6">
          <h2 className="text-xl font-semibold mb-4">Game Rules</h2>
          <ul className="space-y-2 mb-6">
            {game.rules
              .sort((a, b) => a.sortOrder - b.sortOrder)
              .map((rule) => (
                <li key={rule.id} className="flex items-start">
                  <span className="mr-2">•</span>
                  <span>
                    If divisible by{" "}
                    <span className="font-medium">{rule.divisor}</span>, write{" "}
                    <span className="font-medium text-blue-700">
                      - {rule.replaceWord} -
                    </span>
                  </span>
                </li>
              ))}
            <li className="flex items-start">
              <span className="mr-2">•</span>
              <span>
                If not divisible by any of the above, write the number itself
              </span>
            </li>
            <li className="flex items-start">
              <span className="mr-2">•</span>
              <span>
                If divisible by multiple numbers, combine the words in order
              </span>
            </li>
          </ul>

          <div className="border-t border-gray-200 pt-4">
            <h3 className="font-semibold mb-2">Game Duration (seconds)</h3>
            <div className="flex items-center gap-4 mb-2">
              <input
                type="range"
                min="10"
                max="180"
                step="10"
                value={duration}
                onChange={(e) => setDuration(parseInt(e.target.value))}
                className="flex-grow h-2 rounded-lg appearance-none bg-gray-200 cursor-pointer"
                onKeyDown={handleSetupKeyDown}
              />
              <span className="w-12 text-center font-mono">{duration}</span>
            </div>
            <p className="text-sm text-gray-500">
              Choose how long you want to play (10-180 seconds)
            </p>
          </div>
        </Card>

        <div className="flex justify-end">
          <Button onClick={startGame} size="lg">
            Start Game
          </Button>
        </div>
      </div>
    );
  }

  // The game loading UI
  if (gameState === GameState.LOADING) {
    return (
      <div className="max-w-2xl mx-auto text-center py-12">
        <LoadingSpinner size="lg" />
        <p className="mt-4 text-gray-600">Loading game...</p>
      </div>
    );
  }

  // Error state
  if (gameState === GameState.ERROR) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card className="text-center py-8">
          <h2 className="text-xl font-semibold text-red-600 mb-2">Error</h2>
          <p className="text-gray-700 mb-6">{error}</p>
          <Button onClick={() => router.push("/")}>Back to Home</Button>
        </Card>
      </div>
    );
  }

  // Game play UI
  if (
    session &&
    (gameState === GameState.PLAYING ||
      gameState === GameState.WAITING_FOR_ANSWER ||
      gameState === GameState.FEEDBACK)
  ) {
    return (
      <div className="max-w-2xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold">{session.gameName}</h1>

          <div className="flex items-center gap-4">
            <div className="text-center">
              <p className="text-sm text-gray-600">Score</p>
              <p className="font-bold">
                <span className="text-green-600">{session.correctAnswers}</span>
                <span className="mx-1">/</span>
                <span className="text-red-600">{session.incorrectAnswers}</span>
              </p>
              <div className="flex items-center gap-4">
                {/* Existing score and timer */}
                <Button
                  onClick={() => endGame(session?.id)}
                  variant="danger"
                  size="sm"
                >
                  End Game
                </Button>
              </div>
            </div>

            <div className="relative w-20 h-20 flex items-center justify-center bg-white rounded-full shadow-sm">
              <div className="absolute inset-0 rounded-full">
                <svg
                  viewBox="0 0 100 100"
                  className="absolute inset-0 transform rotate-90"
                >
                  <circle
                    cx="50"
                    cy="50"
                    r="45"
                    fill="transparent"
                    stroke="#e5e7eb"
                    strokeWidth="10"
                  />
                  <circle
                    cx="50"
                    cy="50"
                    r="45"
                    fill="transparent"
                    stroke="#3b82f6"
                    strokeWidth="10"
                    strokeDasharray="283"
                    strokeDashoffset={
                      283 * (1 - remainingTime / session.duration)
                    }
                    strokeLinecap="round"
                  />
                </svg>
              </div>
              <span className="text-xl font-mono font-bold">
                {formatTime(remainingTime)}
              </span>
            </div>
          </div>
        </div>

        <Card className="mb-6">
          {gameState === GameState.PLAYING && currentNumber !== null && (
            <div className="text-center py-4">
              <div className="text-6xl font-bold text-blue-600 mb-8">
                {currentNumber}
              </div>

              <form onSubmit={handleSubmit}>
                <div className="max-w-md mx-auto">
                  <label
                    htmlFor="answer"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Your Answer
                  </label>
                  <div className="flex gap-2">
                    <TextInput
                      id="answer"
                      ref={answerInputRef}
                      value={answer}
                      onChange={(e) => setAnswer(e.target.value)}
                      className="text-center text-xl py-3"
                      placeholder="Type your answer..."
                      autoComplete="off"
                      autoFocus
                    />
                    <Button type="submit" disabled={!answer.trim()}>
                      Submit
                    </Button>
                  </div>
                </div>
              </form>
            </div>
          )}

          {gameState === GameState.WAITING_FOR_ANSWER && (
            <div className="text-center py-12">
              <LoadingSpinner />
              <p className="mt-4 text-gray-600">Checking your answer...</p>
            </div>
          )}

          {gameState === GameState.FEEDBACK && answerResult && (
            <div className="text-center py-6">
              <div className="mb-4">
                <Badge
                  variant={answerResult.isCorrect ? "success" : "danger"}
                  className="text-base px-3 py-1"
                >
                  {answerResult.isCorrect ? "Correct!" : "Incorrect!"}
                </Badge>
              </div>

              <div className="mb-6">
                <p className="text-gray-700 mb-1">
                  The number was:{" "}
                  <span className="font-bold">{currentNumber}</span>
                </p>
                <p className="text-gray-700">
                  Correct answer:{" "}
                  <span className="font-bold text-blue-700">
                    {answerResult.correctAnswer}
                  </span>
                </p>
                {!answerResult.isCorrect && (
                  <p className="text-gray-700 mt-1">
                    Your answer: <span className="font-bold">{answer}</span>
                  </p>
                )}
              </div>

              <Button
                onClick={continueGame}
                variant={answerResult.isCorrect ? "success" : "primary"}
              >
                Continue
              </Button>
            </div>
          )}
        </Card>

        <div className="bg-gray-100 rounded-lg p-4">
          <h3 className="font-medium mb-2">Game Rules Reminder:</h3>
          <ul className="text-sm space-y-1">
            {session.rules
              .sort((a, b) => a.sortOrder - b.sortOrder)
              .map((rule) => (
                <li key={rule.id}>
                  If divisible by {rule.divisor}, write - {rule.replaceWord} -
                </li>
              ))}
          </ul>
        </div>
      </div>
    );
  }

  // Fallback UI
  return (
    <div className="max-w-2xl mx-auto text-center py-12">
      <LoadingSpinner size="lg" />
      <p className="mt-4 text-gray-600">Preparing game...</p>
    </div>
  );
}
