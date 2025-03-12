"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { GameResult } from "../../../types";
import { sessionService } from "../../../services/apiService";
import { formatPercentage } from "../../../utils/formatters";
import Card from "../../../components/Card";
import Button from "../../../components/Button";
import LoadingSpinner from "../../../components/LoadingSpinner";

export default function GameResultPage() {
  const router = useRouter();
  const params = useParams();
  const sessionId = params.sessionId as string;

  const [result, setResult] = useState<GameResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchResult = async () => {
      if (!sessionId) return;

      try {
        setLoading(true);
        const resultData = await sessionService.getGameResult(sessionId);
        setResult(resultData);
        console.log("Game result:", resultData);
      } catch (err) {
        console.error("Error fetching game result:", err);
        setError("Failed to load game result. Please try again.");
      } finally {
        setLoading(false);
      }
    };

    fetchResult();
  }, [sessionId]);

  // Loading state
  if (loading) {
    return (
      <div className="max-w-2xl mx-auto text-center py-12">
        <LoadingSpinner size="lg" />
        <p className="mt-4 text-gray-600">Loading results...</p>
      </div>
    );
  }

  // Error state
  if (error) {
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

  // Result display
  if (result) {
    // Determine score color based on accuracy
    const getScoreColor = () => {
      if (result.accuracyPercentage >= 80) return "text-green-600";
      if (result.accuracyPercentage >= 50) return "text-yellow-600";
      return "text-red-600";
    };

    // Get appropriate message based on score
    const getScoreMessage = () => {
      if (result.accuracyPercentage >= 90)
        return "Outstanding! You're a FizzBuzz master!";
      if (result.accuracyPercentage >= 80)
        return "Excellent work! Almost perfect!";
      if (result.accuracyPercentage >= 70) return "Great job! Keep practicing!";
      if (result.accuracyPercentage >= 50)
        return "Good effort! Room for improvement.";
      if (result.accuracyPercentage >= 30)
        return "Nice try! Practice makes perfect.";
      return "Keep practicing! You'll get better with time.";
    };

    return (
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold mb-2">Game Results</h1>
        <p className="text-gray-600 mb-6">{result.gameName}</p>

        <Card className="mb-6">
          <div className="text-center py-6">
            <h2 className="text-2xl font-bold mb-6">Game Summary</h2>

            <div className="flex justify-center mb-8">
              <div className="relative inline-block">
                {/* Circular progress indicator */}
                <svg className="w-48 h-48" viewBox="0 0 100 100">
                  {/* Background circle */}
                  <circle
                    cx="50"
                    cy="50"
                    r="45"
                    fill="transparent"
                    stroke="#e5e7eb"
                    strokeWidth="10"
                  />

                  {/* Score circle */}
                  {result.totalAnswers > 0 && (
                    <circle
                      cx="50"
                      cy="50"
                      r="45"
                      fill="transparent"
                      stroke={
                        result.accuracyPercentage >= 70
                          ? "#10b981"
                          : result.accuracyPercentage >= 40
                          ? "#f59e0b"
                          : "#ef4444"
                      }
                      strokeWidth="10"
                      strokeDasharray="283"
                      strokeDashoffset={
                        283 * (1 - result.accuracyPercentage / 100)
                      }
                      strokeLinecap="round"
                    />
                  )}
                </svg>

                {/* Percentage text in the middle */}
                <div className="absolute inset-0 flex flex-col items-center justify-center">
                  <span className={`text-4xl font-bold ${getScoreColor()}`}>
                    {formatPercentage(result.accuracyPercentage)}
                  </span>
                  <span className="text-sm text-gray-500">Accuracy</span>
                </div>
              </div>
            </div>

            <p className="text-lg mb-8">{getScoreMessage()}</p>

            <div className="grid grid-cols-2 gap-6 max-w-sm mx-auto text-center mb-8">
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-gray-500 text-sm mb-1">Correct</p>
                <p className="text-2xl font-bold text-green-600">
                  {result.correctAnswers}
                </p>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-gray-500 text-sm mb-1">Incorrect</p>
                <p className="text-2xl font-bold text-red-600">
                  {result.incorrectAnswers}
                </p>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-gray-500 text-sm mb-1">Total</p>
                <p className="text-2xl font-bold">{result.totalAnswers}</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-gray-500 text-sm mb-1">Speed</p>
                <p className="text-2xl font-bold">
                  {result.totalAnswers > 0
                    ? `${(result.totalAnswers / 60).toFixed(2)}/s`
                    : "0/s"}
                </p>
              </div>
            </div>
          </div>
        </Card>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Button
            onClick={() => {
              router.push(`/play/${result.gameId}`);
            }}
            variant="primary"
          >
            Play Again
          </Button>
          <Button onClick={() => router.push("/")} variant="secondary">
            Back to Games
          </Button>
        </div>
      </div>
    );
  }

  return null;
}
