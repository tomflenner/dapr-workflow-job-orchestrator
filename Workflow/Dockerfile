FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Workflow/Workflow.csproj", "Workflow/"]
COPY ["Job/Job.csproj", "Job/"]
RUN dotnet restore "Workflow/Workflow.csproj"
COPY . .
WORKDIR "/src/Workflow"
RUN dotnet build "Workflow.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Workflow.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER 1000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Workflow.dll"]
