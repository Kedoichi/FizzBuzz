# Use the official .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY FizzBuzzGame.API/FizzBuzzGame.API.csproj ./FizzBuzzGame.API/
COPY FizzBuzzGame.Core/FizzBuzzGame.Core.csproj ./FizzBuzzGame.Core/
COPY FizzBuzzGame.Infrastructure/FizzBuzzGame.Infrastructure.csproj ./FizzBuzzGame.Infrastructure/

# Restore dependencies
RUN dotnet restore "FizzBuzzGame.API/FizzBuzzGame.API.csproj"

# Copy the entire solution
COPY . .

# Build and publish the API project
WORKDIR "/src/FizzBuzzGame.API"
RUN dotnet publish "FizzBuzzGame.API.csproj" -c Release -o /app/publish

# Use the official .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install culture and timezone support
RUN apt-get update && \
    apt-get install -y tzdata && \
    rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 7056

# Set environment variables
ENV ASPNETCORE_URLS=http://+:7056
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user
RUN addgroup --system --gid 1001 dotnetapp
RUN adduser --system --uid 1001 dotnetapp
USER dotnetapp

# Run the application
ENTRYPOINT ["dotnet", "FizzBuzzGame.API.dll"]