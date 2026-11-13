FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/Repos && chown -R $APP_UID /app/Repos
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Web/ProjectCowork.Api/ProjectCowork.Api.csproj", "Web/ProjectCowork.Api/"]
COPY ["Application/ProjectCowork.Core.Features.Integrations/ProjectCowork.Core.Features.Integrations.csproj", "Application/ProjectCowork.Core.Features.Integrations/"]
COPY ["Domain/ProjectCowork.Domain.Exceptions/ProjectCowork.Domain.Exceptions.csproj", "Domain/ProjectCowork.Domain.Exceptions/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Authorization/ProjectCowork.Infrastructure.Authorization.csproj", "Infrastructure/ProjectCowork.Infrastructure.Authorization/"]
COPY ["Domain/ProjectCowork.Domain/ProjectCowork.Domain.csproj", "Domain/ProjectCowork.Domain/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Abstractions/ProjectCowork.Infrastructure.Abstractions.csproj", "Infrastructure/ProjectCowork.Infrastructure.Abstractions/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Configuration/ProjectCowork.Infrastructure.Configuration.csproj", "Infrastructure/ProjectCowork.Infrastructure.Configuration/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Persistence/ProjectCowork.Infrastructure.Persistence.csproj", "Infrastructure/ProjectCowork.Infrastructure.Persistence/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Domain/ProjectCowork.Infrastructure.Domain.csproj", "Infrastructure/ProjectCowork.Infrastructure.Domain/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.DomainEvents/ProjectCowork.Infrastructure.DomainEvents.csproj", "Infrastructure/ProjectCowork.Infrastructure.DomainEvents/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Integrations.AzureBlob/ProjectCowork.Infrastructure.Integrations.AzureBlob.csproj", "Infrastructure/ProjectCowork.Infrastructure.Integrations.AzureBlob/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Integrations.Github/ProjectCowork.Infrastructure.Integrations.Github.csproj", "Infrastructure/ProjectCowork.Infrastructure.Integrations.Github/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Integrations.Git/ProjectCowork.Infrastructure.Integrations.Git.csproj", "Infrastructure/ProjectCowork.Infrastructure.Integrations.Git/"]
COPY ["Application/ProjectCowork.Core.Features.Management/ProjectCowork.Core.Features.Management.csproj", "Application/ProjectCowork.Core.Features.Management/"]
COPY ["Application/ProjectCowork.Core.Features.Permissions/ProjectCowork.Core.Features.Permissions.csproj", "Application/ProjectCowork.Core.Features.Permissions/"]
COPY ["Application/ProjectCowork.Core.Features.ProjectPosting/ProjectCowork.Core.Features.ProjectPosting.csproj", "Application/ProjectCowork.Core.Features.ProjectPosting/"]
COPY ["Application/ProjectCowork.Core.Features.Project/ProjectCowork.Core.Features.Project.csproj", "Application/ProjectCowork.Core.Features.Project/"]
COPY ["Application/ProjectCowork.Core.Features.Repositories/ProjectCowork.Core.Features.Repositories.csproj", "Application/ProjectCowork.Core.Features.Repositories/"]
COPY ["Application/ProjectCowork.Core.Features.User/ProjectCowork.Core.Features.User.csproj", "Application/ProjectCowork.Core.Features.User/"]
COPY ["Application/ProjectCowork.Core/ProjectCowork.Core.csproj", "Application/ProjectCowork.Core/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure/ProjectCowork.Infrastructure.csproj", "Infrastructure/ProjectCowork.Infrastructure/"]
COPY ["Infrastructure/ProjectCowork.Infrastructure.Integrations/ProjectCowork.Infrastructure.Integrations.csproj", "Infrastructure/ProjectCowork.Infrastructure.Integrations/"]
RUN dotnet restore "Web/ProjectCowork.Api/ProjectCowork.Api.csproj"
COPY . .
WORKDIR "/src/Web/ProjectCowork.Api"
RUN dotnet build "./ProjectCowork.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProjectCowork.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectCowork.Api.dll"]
