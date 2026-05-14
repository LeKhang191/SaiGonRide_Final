FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SaigonRide/*.csproj ./SaigonRide/
RUN dotnet restore ./SaigonRide/
COPY SaigonRide/ ./SaigonRide/
RUN dotnet publish ./SaigonRide/ -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["dotnet", "SaigonRide.dll"]
