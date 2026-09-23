# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Data folder copy for text files
COPY Data/ ./Data/
COPY wwwroot/ ./wwwroot/

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Auto-detect the exact compiled DLL name dynamically
CMD ["sh", "-c", "dotnet $(ls *.dll | head -n 1)"]