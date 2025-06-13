FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution và project file
COPY *.sln .
COPY EngPractice/*.csproj ./EngPractice/

# Restore dependencies
RUN dotnet restore

# Copy toàn bộ mã nguồn
COPY . .

# Build và publish
WORKDIR /app/EngPractice
RUN dotnet publish -c Release -o /out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "EngPractice.dll"]
