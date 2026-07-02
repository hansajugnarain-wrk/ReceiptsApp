FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/ReceiptsApp.Domain/ReceiptsApp.Domain.csproj", "src/ReceiptsApp.Domain/"]
COPY ["src/ReceiptsApp.Application/ReceiptsApp.Application.csproj", "src/ReceiptsApp.Application/"]
COPY ["src/ReceiptsApp.Infrastructure/ReceiptsApp.Infrastructure.csproj", "src/ReceiptsApp.Infrastructure/"]
COPY ["src/ReceiptsApp.Infrastructure.Receipts/ReceiptsApp.Infrastructure.Receipts.csproj", "src/ReceiptsApp.Infrastructure.Receipts/"]
COPY ["src/ReceiptsApp.Api/ReceiptsApp.Api.csproj", "src/ReceiptsApp.Api/"]

RUN dotnet restore "src/ReceiptsApp.Api/ReceiptsApp.Api.csproj"

COPY . .
RUN dotnet publish "src/ReceiptsApp.Api/ReceiptsApp.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ReceiptsApp.Api.dll"]
