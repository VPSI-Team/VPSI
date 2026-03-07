FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Kopírovat csproj a obnovit závislosti
COPY src/HwSimulator.App/HwSimulator.App.csproj src/HwSimulator.App/
RUN dotnet restore src/HwSimulator.App/HwSimulator.App.csproj

# Kopírovat zdrojový kód a sestavit
COPY . .
RUN dotnet publish src/HwSimulator.App/HwSimulator.App.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HwSimulator.App.dll"]
