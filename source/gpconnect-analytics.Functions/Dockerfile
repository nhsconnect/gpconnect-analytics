FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["gpconnect-analytics.Functions/gpconnect-analytics.Functions.csproj", "gpconnect-analytics.Functions/"]
COPY ["gpconnect-analytics.Helpers/gpconnect-analytics.Helpers.csproj", "gpconnect-analytics.Helpers/"]
COPY ["gpconnect-analytics.DAL/gpconnect-analytics.DAL.csproj", "gpconnect-analytics.DAL/"]
COPY ["gpconnect-analytics.DTO/gpconnect-analytics.DTO.csproj", "gpconnect-analytics.DTO/"]
RUN dotnet restore "gpconnect-analytics.Functions/gpconnect-analytics.Functions.csproj"
COPY . .
WORKDIR "/src/gpconnect-analytics.Functions"
RUN dotnet build "gpconnect-analytics.Functions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "gpconnect-analytics.Functions.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV "AzureWebJobsScriptRoot"="/app" \
    "AzureFunctionsJobHost__Logging__Console__IsEnabled"="true" \
    "ConnectionStrings:GPConnectAnalytics"="Server=tcp:cpbisuksqldev.database.windows.net,1433;Initial Catalog=GPConnectAnalytics;Persist Security Info=False;User ID=GpConnectAnalyticsOwnerLogin;Password=#y9BjE9bF4CyLyiCX5Jmjh$q2bsEZD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
