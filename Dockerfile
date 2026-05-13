# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/ModularMonolith/ModularMonolith.csproj", "src/ModularMonolith/"]
RUN dotnet restore "src/ModularMonolith/ModularMonolith.csproj"

COPY . .
WORKDIR "/src/src/ModularMonolith"
RUN dotnet publish "ModularMonolith.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ModularMonolith.dll"]
