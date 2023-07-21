FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["BslLogExporter/BslLogExporter.csproj", "BslLogExporter/"]
COPY ["BslLogExporter.App/BslLogExporter.App.csproj", "BslLogExporter.App/"]
COPY ["BslLogExporter.Core/BslLogExporter.Core.csproj", "BslLogExporter.Core/"]
COPY ["BslLogExporter.CsScript/BslLogExporter.CsScript.csproj", "BslLogExporter.CsScript/"]
COPY ["BslLogExporter.OScript/BslLogExporter.OScript.csproj", "BslLogExporter.OScript/"]

RUN dotnet restore "BslLogExporter/BslLogExporter.csproj"
COPY . .
WORKDIR "/src/BslLogExporter"
RUN dotnet build "BslLogExporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BslLogExporter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BslLogExporter.dll"]
