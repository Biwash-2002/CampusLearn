# Stage 1: Build stage using official .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CampusLearn.csproj", "./"]
RUN dotnet restore "CampusLearn.csproj"

# Copy the remaining project files and build/publish in Release configuration
COPY . .
RUN dotnet publish "CampusLearn.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage using official ASP.NET Core 10 runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Configure ASP.NET Core to listen on 0.0.0.0:8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Copy published artifacts from build stage
COPY --from=build /app/publish .

# Set application entrypoint
ENTRYPOINT ["dotnet", "CampusLearn.dll"]
