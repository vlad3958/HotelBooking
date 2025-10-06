# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files maintaining backend structure
COPY HotelBooking.sln ./
COPY backend/ ./backend/

# Restore dependencies (solution file references backend/ paths)
RUN dotnet restore

# All source code is already copied above

# Build and publish
WORKDIR /src/backend/HotelBooking.API
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]