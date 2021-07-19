FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as dependencies
WORKDIR /build
# Have to copy each of these in individually as Go's filepath.match
# does not support handling wildcard subfolders using ** (eg ./src/**/*.csproj)
COPY ./src/ScoreSys.Api/*.csproj ./ScoreSys.Api/
COPY ./src/ScoreSys.Api.Tests/*.csproj ./ScoreSys.Api.Tests/
COPY ./src/ScoreSys.Entities/*.csproj ./ScoreSys.Entities/
COPY ./src/ScoreSys.Migrations/*.csproj ./ScoreSys.Migrations/
COPY ./src/ScoreSys.Worker/*.csproj ./ScoreSys.Worker/
COPY ./src/*.sln ./
RUN dotnet restore

FROM dependencies as build
COPY ./src/ .
RUN dotnet publish ./ScoreSys.Worker/ScoreSys.Worker.csproj -c Release -o worker

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine as worker
WORKDIR /app
COPY --from=build /build/worker ./
ENTRYPOINT ["dotnet", "ScoreSys.Worker.dll"]
