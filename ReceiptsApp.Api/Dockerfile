FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Disable NuGet fallback packages (fixes Visual Studio-specific paths)
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV NUGET_XMLDOC_MODE=skip
ENV NUGET_CREDENTIALPROVIDERS_READONLY=true
# Or for specific tools like Syncfusion, QuestPDF, etc., if applicable:
# ENV QUESTPDF_LICENSE=Community 

COPY ["ReceiptsApp.Domain/ReceiptsApp.Domain.csproj", "ReceiptsApp.Domain/"]
COPY ["ReceiptsApp.Application/ReceiptsApp.Application.csproj", "ReceiptsApp.Application/"]
COPY ["ReceiptsApp.Infrastructure/ReceiptsApp.Infrastructure.csproj", "ReceiptsApp.Infrastructure/"]
COPY ["ReceiptsApp.Infrastructure.Receipts/ReceiptsApp.Infrastructure.Receipts.csproj", "ReceiptsApp.Infrastructure.Receipts/"]
COPY ["ReceiptsApp.Api/ReceiptsApp.Api.csproj", "ReceiptsApp.Api/"]

RUN dotnet restore "ReceiptsApp.Api/ReceiptsApp.Api.csproj" --disable-parallel --force

COPY . .
RUN rm -rf /src/**/obj && dotnet publish "ReceiptsApp.Api/ReceiptsApp.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ReceiptsApp.Api.dll"]
