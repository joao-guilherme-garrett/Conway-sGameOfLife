FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["GameOfLife.sln", "./"]
COPY ["src/GameOfLife.Api/GameOfLife.Api.csproj", "src/GameOfLife.Api/"]
COPY ["tests/GameOfLife.Tests/GameOfLife.Tests.csproj", "tests/GameOfLife.Tests/"]

RUN dotnet restore "GameOfLife.sln"

COPY . .
WORKDIR "/src/src/GameOfLife.Api"

RUN dotnet build "GameOfLife.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameOfLife.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameOfLife.Api.dll"]
