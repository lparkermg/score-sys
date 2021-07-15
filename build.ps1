param (
    [string]$version = "0.0.0",
    [string]$imageBase = "score-sys"
)

[string]$region = "eu-west-2"

function BuildImages(){
    Write-Host("Building images for version $version");
    Write-Host("===============");
    & docker image build -t $imageBase/api:$version -t $imageBase/api:latest --force-rm -f ./build/api/Dockerfile .
    & docker image build -t $imageBase/worker:$version -t $imageBase/worker:latest --force-rm -f ./build/worker/Dockerfile .
    & docker image build -t $imageBase/migrations:$version -t $imageBase/migrations:latest --force-rm -f ./build/migrations/Dockerfile .
}

function PushImages(){
    Write-Host("Attempting to log in to docker via AWS...")
    aws ecr get-login-password --region ${region} | docker login --username AWS --password-stdin "${Env:AWS_ACCOUNT_ID}.dkr.ecr.${region}.amazonaws.com"
    
    Write-Host("Pushing images");
    Write-Host("==============");

    Write-Host("Versioned images:");
    & docker push $imageBase/api:$version
    & docker push $imageBase/worker:$version
    & docker push $imageBase/migrations:$version

    Write-Host("Latest images:");
    & docker push $imageBase/api:latest
    & docker push $imageBase/worker:latest
    & docker push $imageBase/migrations:latest
}

BuildImages
Write-Host("Images built");

if($imageBase -ne "score-sys"){
    PushImages
    Write-Host("Images pushed");
}