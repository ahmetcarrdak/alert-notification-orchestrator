# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (cache layer)
COPY src/AlertNotificationService.Domain/AlertNotificationService.Domain.csproj             src/AlertNotificationService.Domain/
COPY src/AlertNotificationService.Application/AlertNotificationService.Application.csproj   src/AlertNotificationService.Application/
COPY src/AlertNotificationService.Infrastructure/AlertNotificationService.Infrastructure.csproj src/AlertNotificationService.Infrastructure/
COPY src/AlertNotificationService.API/AlertNotificationService.API.csproj                   src/AlertNotificationService.API/

RUN dotnet restore src/AlertNotificationService.API/AlertNotificationService.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/AlertNotificationService.API/AlertNotificationService.API.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AlertNotificationService.API.dll"]
