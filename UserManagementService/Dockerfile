# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source

# ===>>> CHANGE 1: Copy ONLY the specific .csproj file first <<<===
# Replace 'YourMicroserviceName.csproj' with the actual project file name
COPY UserManagementService.csproj .

# ===>>> CHANGE 2: Restore specifically for this project <<<===
RUN dotnet restore UserManagementService.csproj --use-current-runtime

COPY . .

# ===>>> CHANGE 3: Publish specifically this project <<<===
RUN dotnet publish UserManagementService.csproj -c Release -o /app/publish --no-restore --use-current-runtime

# Stage 2: Create the runtime image (remains the same)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 5161
ENTRYPOINT ["dotnet", "UserManagementService.dll"]