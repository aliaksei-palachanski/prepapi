FROM mcr.microsoft.com/dotnet/sdk:10.0 AS first-stage
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app --no-restore

# Финальная стадия: только рантайм, образ намного меньше
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
USER $APP_UID
COPY --from=first-stage /app .
ENTRYPOINT ["dotnet", "PrepApi.dll"]