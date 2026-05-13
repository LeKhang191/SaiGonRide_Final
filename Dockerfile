FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SaigonRide/SaigonRide.csproj SaigonRide/
RUN dotnet restore SaigonRide/SaigonRide.csproj
COPY SaigonRide/ SaigonRide/
WORKDIR /src/SaigonRide
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Memory limits for Render free tier (512MB)
ENV DOTNET_GCHeapHardLimit=314572800
ENV DOTNET_SYSTEM_GC_CONSERVEMEMORY=9
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 10000
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet SaigonRide.dll"]
