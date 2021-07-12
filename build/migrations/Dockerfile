FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /build
COPY ./src .
RUN dotnet publish ./ScoreSys.Migrations/ScoreSys.Migrations.csproj -c Release -o migrations

FROM mcr.microsoft.com/dotnet/runtime:5.0 as migrations
WORKDIR /app
COPY --from=build /build/migrations ./
ENTRYPOINT ["dotnet", "ScoreSys.Migrations.dll"]