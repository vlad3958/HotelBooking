# Use the official .NET 9 SDK image# Use the official .NET 9 runtime image

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS buildFROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /appWORKDIR /app

EXPOSE 8080

# Copy solution file and project files

COPY HotelBooking.sln ./# Use the official .NET 9 SDK image for building

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Copy project filesWORKDIR /src

COPY HotelBooking.API/HotelBooking.API.csproj ./HotelBooking.API/

COPY HotelBooking.Application/HotelBooking.Application.csproj ./HotelBooking.Application/# Copy solution file and project files maintaining backend structure

COPY HotelBooking.Domain/HotelBooking.Domain.csproj ./HotelBooking.Domain/COPY HotelBooking.sln ./

COPY HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj ./HotelBooking.Infrastructure/COPY backend/ ./backend/



# Restore dependencies# Restore dependencies (solution file references backend/ paths)

RUN dotnet restoreRUN dotnet restore



# Copy the rest of the source code# All source code is already copied above

COPY . .

# Build and publish

# Build the applicationWORKDIR /src/backend/HotelBooking.API

RUN dotnet publish HotelBooking.API/HotelBooking.API.csproj -c Release -o /app/publishRUN dotnet publish -c Release -o /app/publish



# Runtime stage# Final stage

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtimeFROM base AS final

WORKDIR /appWORKDIR /app

COPY --from=build /app/publish .

# Copy the published app

COPY --from=build /app/publish .# Render uses PORT environment variable

ENV ASPNETCORE_URLS=http://+:$PORT

# Expose port 8080 (App Runner default)ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]
# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]