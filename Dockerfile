# Build stage: runs on the builder's own platform (amd64 on the dev PC), never
# under QEMU emulation. The SDK cross-publishes for linux-arm64: every NuGet
# package of the solution is managed code, so no native toolchain is needed.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# .NET runtime identifier of the server (overridable: --build-arg RID=linux-x64)
ARG RID=linux-arm64
WORKDIR /src

# Copy solution and project files for layer caching
COPY Cosmere.sln .
COPY API/API.csproj API/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Services/Services.csproj Services/
COPY Cross/Cross.csproj Cross/
COPY Messages/Messages.csproj Messages/

# Restore dependencies for the target runtime (linux-arm64 on the server)
RUN dotnet restore Cosmere.sln -r $RID -p:UseAppHost=false

# Copy remaining source
COPY . .

# Publish framework-dependent for the target runtime (entrypoint is `dotnet API.dll`, no apphost)
RUN dotnet publish API/API.csproj -c Release -r $RID --self-contained false -p:UseAppHost=false --no-restore -o /app/publish

# Runtime stage: built for the target platform. No RUN here, so the target
# platform never needs emulation.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5200

EXPOSE 5200

ENTRYPOINT ["dotnet", "API.dll"]
