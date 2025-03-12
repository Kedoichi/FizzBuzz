/* eslint-disable @typescript-eslint/no-explicit-any */
export interface GameRule {
  id: string;
  divisor: number;
  replaceWord: string;
  sortOrder: number;
}

export interface Game {
  id: string;
  name: string;
  author: string;
  dateCreated: string;
  rules: GameRule[];
}

export interface CreateGameRule {
  divisor: number;
  replaceWord: string;
  sortOrder: number;
}

export interface CreateGame {
  name: string;
  author: string;
  rules: CreateGameRule[];
}

export interface GameSession {
  id: string;
  gameId: string;
  ssID: string;
  gameName: string;
  startTime: string;
  duration: number;
  remainingTime: number;
  correctAnswers: number;
  incorrectAnswers: number;
  rules: GameRule[];
}

export interface CreateSession {
  gameId: string;
  duration: number;
}

export interface GameRound {
  number: number;
}

export interface Answer {
  answer: string;
}

export interface AnswerResult {
  isCorrect: boolean;
  correctAnswer: string;
  isGameOver: boolean;
}

export interface GameResult {
  gameId: any;
  id: string;
  gameName: string;
  totalAnswers: number;
  correctAnswers: number;
  incorrectAnswers: number;
  accuracyPercentage: number;
  isCompleted: boolean;
}
