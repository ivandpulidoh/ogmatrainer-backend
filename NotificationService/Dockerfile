# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

# ===>>> CHANGE 1: Copy ONLY the specific .csproj file first <<<===
# Replace 'YourMicroserviceName.csproj' with the actual project file name
COPY NotificationService.csproj .

# ===>>> CHANGE 2: Restore specifically for this project <<<===
RUN dotnet restore NotificationService.csproj --runtime linux-musl-x64

COPY . .

# ===>>> CHANGE 3: Publish specifically this project <<<===
RUN dotnet publish NotificationService.csproj -c Release -o /app/publish --runtime linux-musl-x64 --no-self-contained

# Stage 2: Create the runtime image (remains the same)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 5092
ENTRYPOINT ["dotnet", "NotificationService.dll"]