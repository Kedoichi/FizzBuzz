# FizzBuzz Game 🎮

## Overview

FizzBuzz Game is an interactive web application that puts a fun twist on the classic programming interview question. Players can create custom FizzBuzz-style games with unique rules and test their skills by answering as many correct questions as possible within a time limit.

## 🚀 Features

-   **Custom Game Creation**: Design your own FizzBuzz-like game with custom divisibility rules
-   **Timed Gameplay**: Challenge yourself with adjustable game duration
-   **Scoring System**: Track correct and incorrect answers
-   **Responsive Design**: Play on desktop and mobile devices
-   **Real-time Feedback**: Instant validation of your answers

## 🛠 Technologies

### Frontend

-   Next.js 14
-   React
-   TypeScript
-   Tailwind CSS

### Backend

-   .NET 8
-   ASP.NET Core
-   MongoDB

### DevOps

-   Docker
-   Docker Compose

## 📦 Prerequisites

-   Node.js 20+
-   .NET 8 SDK
-   Docker (optional)
-   MongoDB (optional)

## 🔧 Local Development Setup

### Backend Setup

```bash
# Navigate to API project
cd FizzBuzzGame.API

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

## 🐳 Docker Deployment

```bash
# Build and run all services
docker build -t backend -f FizzBuzzGame.API/Dockerfile .

docker-compose up --build

# Stop services
docker-compose down
```

## 🧪 Running Tests

### Backend Tests

```bash
cd FizzBuzzGame.Tests
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```

## 📞 Contact

Your Name - Quoc T Vu - [quocchic.net](http://quochcic.net/)

Project Link: [[[https://github.com/yourusername/FizzBuzzGame](https://github.com/yourusername/FizzBuzzGame)](https://github.com/Kedoichi/FizzBuzz)](https://github.com/Kedoichi/FizzBuzz)

## 🌟 Acknowledgements

-   [Next.js](https://nextjs.org/)
-   [.NET](https://dotnet.microsoft.com/)
-   [MongoDB](https://www.mongodb.com/)
-   [Docker](https://www.docker.com/)

---

**Happy Coding!** 👨‍💻👩‍💻
