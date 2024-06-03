# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY MongoProject/*.csproj ./MongoProject/
RUN dotnet restore

# copy everything else and build app
COPY MongoProject/. ./MongoProject/
WORKDIR /source/MongoProject
RUN dotnet publish -c release -o /app

EXPOSE 5209
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "MongoProject.dll"]