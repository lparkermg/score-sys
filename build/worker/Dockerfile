FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /build
COPY ./src .
RUN dotnet publish ./ScoreSys.Worker/ScoreSys.Worker.csproj -c Release -o worker

FROM mcr.microsoft.com/dotnet/runtime:5.0 as worker
WORKDIR /app
COPY --from=build /build/worker ./
ENTRYPOINT ["dotnet", "ScoreSys.Worker.dll"]
