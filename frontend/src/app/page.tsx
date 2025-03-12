"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Game } from "../types";
import { gameService } from "../services/apiService";
import GameCard from "../components/GameCard";
import LoadingSpinner from "../components/LoadingSpinner";

export default function HomePage() {
  const [games, setGames] = useState<Game[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchGames = async () => {
      try {
        setLoading(true);
        const data = await gameService.getGames();
        setGames(data);
        setError(null);
      } catch (err) {
        console.error("Error fetching games:", err);
        setError(
          err instanceof Error
            ? err.message
            : "Failed to load games. Please try again later."
        );
      } finally {
        setLoading(false);
      }
    };

    fetchGames();
  }, []);

  return (
    <div>
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">FizzBuzz Games</h1>
          <p className="text-gray-600 mt-1">
            Select a game to play or create your own!
          </p>
        </div>
        <Link
          href="/create"
          className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors shadow-sm"
        >
          Create New Game
        </Link>
      </div>

      {loading ? (
        <div className="flex justify-center py-12">
          <LoadingSpinner />
        </div>
      ) : error ? (
        <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded-md">
          <p className="font-medium">Error</p>
          <p>{error}</p>
        </div>
      ) : games.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm p-8 text-center">
          <h2 className="text-xl font-semibold mb-2">No Games Available</h2>
          <p className="text-gray-600 mb-6">
            Create your first game to get started!
          </p>
          <Link
            href="/create"
            className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors inline-block"
          >
            Create a Game
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {games.map((game) => (
            <GameCard key={game.id} game={game} />
          ))}
        </div>
      )}
    </div>
  );
}
