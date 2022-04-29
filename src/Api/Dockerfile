#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . ./
WORKDIR /src/Api
RUN dotnet restore "Api.csproj"
RUN dotnet build "Api.csproj" -c Release -o /build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /publish

FROM base AS final
WORKDIR /app
COPY --from=publish /publish .
COPY --from=publish /publish/appsettings.json ../config/appsettings.json
ENV MediaLibrarySettings__SourceDirectory=/books
ENTRYPOINT ["dotnet", "Auricular.Api.dll", "config_dir=/config"]

VOLUME /config