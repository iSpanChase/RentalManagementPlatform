# syntax=docker/dockerfile:1

# 1. Restore Stage: Restore dependencies first for better layer caching.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy the solution file and all project files first.
# This allows Docker to cache the restore layer effectively.
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformMVC.sln", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/"]
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformWebAPI/RentalManagementPlatformWebAPI.csproj", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformWebAPI/"]
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatform.Common/RentalManagementPlatform.Common.csproj", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatform.Common/"]
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/ChaseCheng.Global.Utilities/ChaseCheng.Global.Utilities.csproj", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/ChaseCheng.Global.Utilities/"]
# The .sln file also references AutoFK and the main MVC project, let's copy them too for a clean restore.
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformMVC.csproj", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformMVC/"]
COPY ["RentalManagementPlatformMVC/RentalManagementPlatformMVC/AutoFK/AutoFK.csproj", "RentalManagementPlatformMVC/RentalManagementPlatformMVC/AutoFK/"]


# Restore dependencies for the entire solution.
RUN dotnet restore "RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformMVC.sln"

# 2. Build and Publish Stage: Build and publish the application.
FROM restore AS publish
WORKDIR /src

# Copy the rest of the source code. The .dockerignore file will prevent obj/bin from the host.
COPY . .

# Set the working directory and run publish.
WORKDIR "/src/RentalManagementPlatformMVC/RentalManagementPlatformMVC/RentalManagementPlatformWebAPI"
RUN dotnet publish "RentalManagementPlatformWebAPI.csproj" -c Release -o /app/publish --no-restore

# 3. Final Stage: Create the final, lean image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentalManagementPlatformWebAPI.dll"]
