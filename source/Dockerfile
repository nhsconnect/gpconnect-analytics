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
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true