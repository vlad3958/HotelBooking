# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files from backend folder
COPY HotelBooking.sln ./
COPY backend/HotelBooking.API/HotelBooking.API.csproj HotelBooking.API/
COPY backend/HotelBooking.Application/HotelBooking.Application.csproj HotelBooking.Application/
COPY backend/HotelBooking.Domain/HotelBooking.Domain.csproj HotelBooking.Domain/
COPY backend/HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj HotelBooking.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the backend source code
COPY backend/ .

# Build and publish
WORKDIR /src/HotelBooking.API
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]