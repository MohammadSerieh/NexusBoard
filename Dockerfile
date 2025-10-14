# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["NexusBoard.API/NexusBoard.API.csproj", "NexusBoard.API/"]
COPY ["NexusBoard.Core/NexusBoard.Core.csproj", "NexusBoard.Core/"]
COPY ["NexusBoard.Infrastructure/NexusBoard.Infrastructure.csproj", "NexusBoard.Infrastructure/"]

RUN dotnet restore "NexusBoard.API/NexusBoard.API.csproj"

# Copy the rest of the code
COPY . .

# Build the application
WORKDIR "/src/NexusBoard.API"
RUN dotnet build "NexusBoard.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NexusBoard.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NexusBoard.API.dll"]