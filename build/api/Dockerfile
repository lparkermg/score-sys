FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /build
COPY ./src .
RUN dotnet publish ./ScoreSys.Api/ScoreSys.Api.csproj -c Release -o api
RUN dotnet test --results-directory /testresults --logger "trx;LogFileName=test_results.xml" ./ScoreSys.Api.Tests/ScoreSys.Api.Tests.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0 as api
WORKDIR /app
COPY --from=build /build/api ./
ENTRYPOINT ["dotnet", "ScoreSys.Api.dll"]

