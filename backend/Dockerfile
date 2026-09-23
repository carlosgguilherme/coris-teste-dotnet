# Etapa 1: compila a API com o SDK do .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CorisSeguros.Api/CorisSeguros.Api.csproj CorisSeguros.Api/
RUN dotnet restore CorisSeguros.Api/CorisSeguros.Api.csproj
COPY CorisSeguros.Api/ CorisSeguros.Api/
RUN dotnet publish CorisSeguros.Api/CorisSeguros.Api.csproj -c Release -o /app

# Etapa 2: roda só com o runtime (imagem menor)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CorisSeguros.Api.dll"]
