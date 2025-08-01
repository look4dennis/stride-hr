# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/StrideHR.API/StrideHR.API.csproj", "src/StrideHR.API/"]
COPY ["src/StrideHR.Core/StrideHR.Core.csproj", "src/StrideHR.Core/"]
COPY ["src/StrideHR.Infrastructure/StrideHR.Infrastructure.csproj", "src/StrideHR.Infrastructure/"]
RUN dotnet restore "src/StrideHR.API/StrideHR.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/StrideHR.API"
RUN dotnet build "StrideHR.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StrideHR.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create logs directory
RUN mkdir -p /app/logs

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StrideHR.API.dll"]