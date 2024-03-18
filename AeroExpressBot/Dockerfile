FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AeroExpressBot/AeroExpressBot.csproj", "AeroExpressBot/"]
COPY ["FileProcessing/FileProcessing.csproj", "FileProcessing/"]
RUN dotnet restore "AeroExpressBot/AeroExpressBot.csproj"
COPY . .
WORKDIR "/src/AeroExpressBot"
RUN dotnet build "AeroExpressBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AeroExpressBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AeroExpressBot.dll"]
