/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  Game,
  CreateGame,
  GameSession,
  CreateSession,
  GameRound,
  Answer,
  AnswerResult,
  GameResult,
} from "../types";

// Determine the base URL based on the environment
// Log error details for better debugging
const logError = (context: string, error: any) => {
  console.error(`[API Error - ${context}]`, {
    message: error.message,
    stack: error.stack,
    timestamp: new Date().toISOString(),
  });
};

// Base API request function with error handling and logging
async function apiRequest<T>(
  url: string,
  options: RequestInit = {}
): Promise<T> {
  const baseUrl = "http://localhost:7056";
  const fullUrl = `${baseUrl}/api${url}`;

  try {
    const response = await fetch(fullUrl, {
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      ...options,
    });

    // Log the response status
    if (!response.ok) {
      let errorData;
      try {
        errorData = await response.json();
      } catch {
        errorData = { message: "An unexpected error occurred" };
      }

      // Log the error message from the API
      console.error(`[API Error] ${response.status} ${fullUrl}`, errorData);

      throw new Error(
        errorData.message ||
          `API request failed with status: ${response.status} ${response.statusText} for URL: ${fullUrl} `
      );
    }

    // Check if the response has content before trying to parse
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) {
      const responseBody = await response.json();
      return responseBody as T;
    }

    // Return an empty object or null for non-JSON responses
    return {} as T;
  } catch (error) {
    // Log any errors during the API request
    console.error("API Request Error:", error);
    logError("API Request", error);
    throw error;
  }
}

// Game endpoints with logging
export const gameService = {
  getGames: async (): Promise<Game[]> => {
    return apiRequest<Game[]>("/Games");
  },

  getGame: async (id: string): Promise<Game> => {
    return apiRequest<Game>(`/Games/${id}`);
  },

  createGame: async (game: CreateGame): Promise<Game> => {
    return apiRequest<Game>("/Games", {
      method: "POST",
      body: JSON.stringify(game),
    });
  },

  updateGame: async (id: string, game: CreateGame): Promise<void> => {
    return apiRequest(`/Games/${id}`, {
      method: "PUT",
      body: JSON.stringify(game),
    });
  },

  deleteGame: async (id: string): Promise<void> => {
    return apiRequest(`/Games/${id}`, {
      method: "DELETE",
    });
  },
};

// Game Session endpoints with logging
export const sessionService = {
  startSession: async (session: CreateSession): Promise<GameSession> => {
    return apiRequest<GameSession>("/GameSessions", {
      method: "POST",
      body: JSON.stringify(session),
    });
  },

  getSession: async (id: string): Promise<GameSession> => {
    return apiRequest<GameSession>(`/GameSessions/${id}`);
  },

  getNextNumber: async (id: string): Promise<GameRound> => {
    return apiRequest<GameRound>(`/GameSessions/${id}/next`);
  },

  submitAnswer: async (id: string, answer: Answer): Promise<AnswerResult> => {
    return apiRequest<AnswerResult>(`/GameSessions/${id}/answer`, {
      method: "POST",
      body: JSON.stringify(answer),
    });
  },

  getGameResult: async (id: string): Promise<GameResult> => {
    return apiRequest<GameResult>(`/GameSessions/${id}/result`);
  },
};
