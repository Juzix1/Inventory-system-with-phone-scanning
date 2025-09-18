# Use the official .NET 9 SDK image for build and migration
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Apply EF Core migrations using SDK image
FROM build AS migration
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef database update --no-build --project InventoryAPI.csproj --startup-project InventoryAPI.csproj

# Use the official .NET 9 ASP.NET runtime image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update && apt-get install -y fonts-dejavu-core
RUN mkdir -p /app/Images
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "InventoryAPI.dll"]