FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as dependencies
WORKDIR /build
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
RUN adduser --disabled-password --home /app --gecos '' app && chown -R app /app
USER app
WORKDIR /app
COPY --from=build /build/worker ./
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "ScoreSys.Worker.dll"]
