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
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "SaigonRide.dll"]
