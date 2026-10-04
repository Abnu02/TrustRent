FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TrustRent.Api/TrustRent.Api.csproj", "TrustRent.Api/"]
COPY ["TrustRent.Application/TrustRent.Application.csproj", "TrustRent.Application/"]
COPY ["TrustRent.Domain/TrustRent.Domain.csproj", "TrustRent.Domain/"]
COPY ["TrustRent.Infrastructure/TrustRent.Infrastructure.csproj", "TrustRent.Infrastructure/"]
RUN dotnet restore "TrustRent.Api/TrustRent.Api.csproj"

COPY . .
WORKDIR /src/TrustRent.Api
RUN dotnet publish "TrustRent.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish
RUN rm -f /app/publish/appsettings*.json

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet TrustRent.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
