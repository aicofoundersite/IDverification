FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Install Node.js
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src
COPY ["CrossSetaWeb.csproj", "."]
RUN dotnet restore "CrossSetaWeb.csproj"
COPY . .

# Build React App
WORKDIR "/src/ClientApp"
RUN npm install
RUN npm run build

# Build .NET App
WORKDIR "/src"
RUN dotnet build "CrossSetaWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrossSetaWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrossSetaWeb.dll"]
