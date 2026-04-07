FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY Tornois.slnx ./
COPY src/Tornois.Api/Tornois.Api.csproj src/Tornois.Api/
COPY src/Tornois.Application/Tornois.Application.csproj src/Tornois.Application/
COPY src/Tornois.Domain/Tornois.Domain.csproj src/Tornois.Domain/
COPY src/Tornois.Infrastructure/Tornois.Infrastructure.csproj src/Tornois.Infrastructure/
RUN dotnet restore src/Tornois.Api/Tornois.Api.csproj

COPY . .
RUN dotnet publish src/Tornois.Api/Tornois.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Tornois.Api.dll"]
