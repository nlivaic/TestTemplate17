#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TestTemplate17.WorkerServices/TestTemplate17.WorkerServices.csproj", "src/TestTemplate17.WorkerServices/"]
COPY ["src/TestTemplate17.Core/TestTemplate17.Core.csproj", "src/TestTemplate17.Core/"]
COPY ["src/TestTemplate17.Common/TestTemplate17.Common.csproj", "src/TestTemplate17.Common/"]
RUN dotnet restore "src/TestTemplate17.WorkerServices/TestTemplate17.WorkerServices.csproj"
COPY . .
WORKDIR "/src/src/TestTemplate17.WorkerServices"
RUN dotnet build "TestTemplate17.WorkerServices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestTemplate17.WorkerServices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["src/TestTemplate17.WorkerServices/entrypoint.sh", "src/entrypoint.sh"]
COPY ["src/TestTemplate17.WorkerServices/wait-for-it.sh", "src/wait-for-it.sh"]
CMD ["/bin/bash", "src/entrypoint.sh"]
