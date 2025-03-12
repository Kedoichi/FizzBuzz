import Link from "next/link";
import { Game } from "../types";
import { formatDate } from "../utils/formatters";

interface GameCardProps {
  game: Game;
}

export default function GameCard({ game }: GameCardProps) {
  // Sort rules by their sortOrder
  const sortedRules = [...game.rules].sort((a, b) => a.sortOrder - b.sortOrder);

  return (
    <div className="bg-white rounded-lg shadow-sm overflow-hidden hover:shadow-md transition-shadow">
      <div className="p-6">
        <h2 className="text-xl font-semibold mb-2 text-gray-900">
          {game.name}
        </h2>
        <p className="text-gray-600 text-sm mb-1">Created by: {game.author}</p>
        <p className="text-gray-500 text-sm mb-4">
          {formatDate(game.dateCreated)}
        </p>

        <div className="mb-4">
          <h3 className="font-medium text-sm text-gray-500 uppercase tracking-wide mb-2">
            Rules:
          </h3>
          <ul className="space-y-1.5">
            {sortedRules.map((rule) => (
              <li
                key={rule.id}
                className="text-gray-700 text-sm flex items-start"
              >
                <span className="mr-2">•</span>
                <span>
                  If divisible by{" "}
                  <span className="font-medium">{rule.divisor}</span>, write{" "}
                  <span className="font-medium text-blue-700">
                    {`<${rule.replaceWord}>`}
                  </span>
                </span>
              </li>
            ))}
          </ul>
        </div>

        <div className="mt-6">
          <Link
            href={`/play/${game.id}`}
            className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition-colors w-full block text-center"
          >
            Play Game
          </Link>
        </div>
      </div>
    </div>
  );
}
