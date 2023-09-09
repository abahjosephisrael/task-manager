#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TaskManager.Presentation.Api/TaskManager.Presentation.Api.csproj", "TaskManager.Presentation.Api/"]
RUN dotnet restore "TaskManager.Presentation.Api/TaskManager.Presentation.Api.csproj"
COPY . .
WORKDIR "/src/TaskManager.Presentation.Api"
RUN dotnet build "TaskManager.Presentation.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.Presentation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Presentation.Api.dll"]