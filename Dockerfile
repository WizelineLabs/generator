FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
# Install NodeJS and Prettier:
RUN (apt-get update || true) && \
    apt-get install -y \
    curl
RUN (curl -sL https://deb.nodesource.com/setup_14.x | bash -) && \
    apt-get install -y nodejs
RUN npm i -g prettier

WORKDIR /app
# COPY templates/* ./templates/
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY *.sln .
COPY Generator.API/*.csproj ./Generator.API/
COPY Reusable/*.csproj ./Reusable/
RUN dotnet restore
COPY . .
WORKDIR "/src/Generator.API/."
RUN dotnet build -c Release -o /app/build


FROM build AS publish
RUN dotnet publish -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Generator.API.dll"]
