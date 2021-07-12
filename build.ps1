param (
    [string]$version = "0.0.0"
)

function BuildImages(){
    Write-Host("Building API image");
    & docker image build -t score-sys/api:$version -t score-sys/api:latest --force-rm -f ./build/api/Dockerfile .

    Write-Host("Building Worker image");
    & docker image build -t score-sys/worker:$version -t score-sys/worker:latest --force-rm -f ./build/worker/Dockerfile .

    Write-Host("Building Migrations image");
    & docker image build -t score-sys/migrations:$version -t score-sys/migrations:latest --force-rm -f ./build/migrations/Dockerfile .
} 

BuildImages

Write-Host("Build complete");