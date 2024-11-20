# Use the base image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Set the app UID (optional if needed, adjust accordingly)
USER $APP_UID
WORKDIR /app
# Expose both HTTP (5000) and HTTPS (5001)
EXPOSE 8080
EXPOSE 8081

# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskManagementSystem.csproj", "./"]
RUN dotnet restore "TaskManagementSystem.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "TaskManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app to the /app/publish folder
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - set the base image and copy published files
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Set entry point to start the web API
ENTRYPOINT ["dotnet", "TaskManagementSystem.dll"]
