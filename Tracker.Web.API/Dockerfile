FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Tracker.API/Tracker.API.csproj", "Tracker.API/"]
RUN dotnet restore "Tracker.API/Tracker.API.csproj"
COPY . .
WORKDIR "/src/Tracker.API"
RUN dotnet build "Tracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tracker.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tracker.API.dll"]
