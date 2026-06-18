FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN find . -name "*.csproj" | head -5
RUN dotnet publish GameNotCrazy.API/GameNotCrazy.API.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .
EXPOSE 8080
CMD ["dotnet", "GameNotCrazy.API.dll"]
