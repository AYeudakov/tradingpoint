# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json BitcoinPrices.sln ./
COPY src/BitcoinPrices.Api/BitcoinPrices.Api.csproj src/BitcoinPrices.Api/
COPY src/BitcoinPrices.Application/BitcoinPrices.Application.csproj src/BitcoinPrices.Application/
COPY src/BitcoinPrices.Domain/BitcoinPrices.Domain.csproj src/BitcoinPrices.Domain/
COPY src/BitcoinPrices.Infrastructure/BitcoinPrices.Infrastructure.csproj src/BitcoinPrices.Infrastructure/
RUN dotnet restore src/BitcoinPrices.Api/BitcoinPrices.Api.csproj

COPY src/ src/
RUN dotnet publish src/BitcoinPrices.Api/BitcoinPrices.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /data && chown -R $APP_UID /data
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
VOLUME ["/data"]

USER $APP_UID
ENTRYPOINT ["dotnet", "BitcoinPrices.Api.dll"]
