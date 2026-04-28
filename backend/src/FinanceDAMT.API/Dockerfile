FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FinanceDAMT.API/FinanceDAMT.API.csproj", "src/FinanceDAMT.API/"]
COPY ["src/FinanceDAMT.Application/FinanceDAMT.Application.csproj", "src/FinanceDAMT.Application/"]
COPY ["src/FinanceDAMT.Infrastructure/FinanceDAMT.Infrastructure.csproj", "src/FinanceDAMT.Infrastructure/"]
COPY ["src/FinanceDAMT.Domain/FinanceDAMT.Domain.csproj", "src/FinanceDAMT.Domain/"]

RUN dotnet restore "src/FinanceDAMT.API/FinanceDAMT.API.csproj"

COPY . .

RUN dotnet build "src/FinanceDAMT.API/FinanceDAMT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/FinanceDAMT.API/FinanceDAMT.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceDAMT.API.dll"]
