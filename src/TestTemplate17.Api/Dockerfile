#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TestTemplate17.Api/TestTemplate17.Api.csproj", "src/TestTemplate17.Api/"]
COPY ["src/TestTemplate17.Data/TestTemplate17.Data.csproj", "src/TestTemplate17.Data/"]
COPY ["src/TestTemplate17.Common/TestTemplate17.Common.csproj", "src/TestTemplate17.Common/"]
COPY ["src/TestTemplate17.Core/TestTemplate17.Core.csproj", "src/TestTemplate17.Core/"]
RUN dotnet restore "src/TestTemplate17.Api/TestTemplate17.Api.csproj"
COPY . .
WORKDIR "/src/src/TestTemplate17.Api"
RUN dotnet build "TestTemplate17.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestTemplate17.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestTemplate17.Api.dll"]