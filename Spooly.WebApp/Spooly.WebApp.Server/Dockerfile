# Build context: repo root  (DockerfileContext = ..\..\)

# ── Stage 1: Build Angular ────────────────────────────────────────────────────
FROM node:22-alpine AS angular-build
WORKDIR /client

# Install dependencies (cached unless package-lock changes)
COPY Spooly.WebApp/spooly.webapp.client/package*.json ./
RUN npm ci --ignore-scripts

# Build
COPY Spooly.WebApp/spooly.webapp.client/ ./
RUN npm run build
# Output: /client/dist/spooly.webapp.client/browser/

# ── Stage 2: Build & publish .NET backend ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /src

# Restore (cached unless .csproj files change)
COPY Spooly/Spooly.csproj                                                       Spooly/
COPY Spooly.DAL/Spooly.DAL.csproj                                               Spooly.DAL/
COPY Spooly.DAL.Security/Spooly.DAL.Security.csproj                             Spooly.DAL.Security/
COPY Spooly.Application/Spooly.Application.csproj                               Spooly.Application/
COPY Spooly.WebApp/Spooly.WebApp.Server/Spooly.WebApp.Server.csproj             Spooly.WebApp/Spooly.WebApp.Server/
# .esproj stub needed so MSBuild can resolve the project reference during restore
COPY Spooly.WebApp/spooly.webapp.client/spooly.webapp.client.esproj             Spooly.WebApp/spooly.webapp.client/

RUN dotnet restore Spooly.WebApp/Spooly.WebApp.Server/Spooly.WebApp.Server.csproj \
    /p:BuildNodeProject=false

# Copy source
COPY Spooly/                        Spooly/
COPY Spooly.DAL/                    Spooly.DAL/
COPY Spooly.DAL.Security/           Spooly.DAL.Security/
COPY Spooly.Application/            Spooly.Application/
COPY Spooly.WebApp/Spooly.WebApp.Server/ Spooly.WebApp/Spooly.WebApp.Server/

# Inject Angular dist into wwwroot so IncludeAngularDist target picks it up
COPY --from=angular-build /client/dist/spooly.webapp.client/browser/ \
     Spooly.WebApp/spooly.webapp.client/dist/spooly.webapp.client/browser/

RUN dotnet publish Spooly.WebApp/Spooly.WebApp.Server/Spooly.WebApp.Server.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:BuildNodeProject=false

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Run as non-root (app user is built into the base image)
USER $APP_UID

COPY --from=dotnet-build /app/publish .
ENTRYPOINT ["dotnet", "Spooly.WebApp.Server.dll"]
