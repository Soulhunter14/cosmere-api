# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY Cosmere.sln .
COPY API/API.csproj API/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Services/Services.csproj Services/
COPY Cross/Cross.csproj Cross/
COPY Messages/Messages.csproj Messages/

# Restore dependencies
RUN dotnet restore Cosmere.sln

# Copy remaining source
COPY . .

# Publish
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5200

EXPOSE 5200

ENTRYPOINT ["dotnet", "API.dll"]
