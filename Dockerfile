FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobApplicationManagement.API/JobApplicationManagement.API.csproj", "JobApplicationManagement.API/"]
COPY ["JobApplicationManagement.Application/JobApplicationManagement.Application.csproj", "JobApplicationManagement.Application/"]
COPY ["JobApplicationManagement.Domain/JobApplicationManagement.Domain.csproj", "JobApplicationManagement.Domain/"]
COPY ["JobApplicationManagement.Infrastucture/JobApplicationManagement.Infrastucture.csproj", "JobApplicationManagement.Infrastucture/"]
RUN dotnet restore "./JobApplicationManagement.API/JobApplicationManagement.API.csproj"
COPY . .
WORKDIR "/src/JobApplicationManagement.API"
RUN dotnet build "./JobApplicationManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JobApplicationManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobApplicationManagement.API.dll"]
