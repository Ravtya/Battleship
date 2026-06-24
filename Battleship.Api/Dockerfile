FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Battleship.Contracts/Battleship.Contracts.csproj Battleship.Contracts/
COPY Battleship.Api/Battleship.Api.csproj Battleship.Api/
RUN dotnet restore Battleship.Api/Battleship.Api.csproj
COPY . .
RUN dotnet publish Battleship.Api/Battleship.Api.csproj -c Release -o /app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Battleship.Api.dll"]