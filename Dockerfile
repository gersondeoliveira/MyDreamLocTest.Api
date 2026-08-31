# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MyDream.Api.csproj ./
RUN dotnet restore "MyDream.Api.csproj"

COPY . .
RUN dotnet publish "MyDream.Api.csproj" -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Cloud Run injeta a porta a ser usada via variável de ambiente PORT.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyDream.Api.dll"]
