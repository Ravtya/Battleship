FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Battleship.Contracts/Battleship.Contracts.csproj Battleship.Contracts/
COPY Battleship.Blazor/Battleship.Blazor.csproj Battleship.Blazor/
RUN dotnet restore Battleship.Blazor/Battleship.Blazor.csproj
COPY . .
RUN dotnet publish Battleship.Blazor/Battleship.Blazor.csproj -c Release -o /app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Battleship.Blazor.dll"]