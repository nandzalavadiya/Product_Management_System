# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["CRN_Technical_Assessment.csproj", "./"]
RUN dotnet restore "CRN_Technical_Assessment.csproj"

# Copy remaining source and build
COPY . .

# Exclude test directory from build
RUN dotnet build "CRN_Technical_Assessment.csproj" -c Release -o /app/build \
    --no-restore

# Stage 2 — Publish
FROM build AS publish
RUN dotnet publish "CRN_Technical_Assessment.csproj" -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

# Stage 3 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CRN_Technical_Assessment.dll"]
