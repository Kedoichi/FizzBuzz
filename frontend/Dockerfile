# Use official Node.js image as base
FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm ci

# Copy the rest of the application
COPY . .

# Set the environment variable to disable telemetry
ENV NEXT_TELEMETRY_DISABLED 1

# Expose the development port
EXPOSE 3000

# Start the development server instead of building
CMD ["npm", "run", "dev"]