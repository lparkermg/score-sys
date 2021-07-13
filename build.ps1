param (
    [string]$version = "0.0.0",
    [string]$imageBase = "score-sys"
)

# TODO: Uncomment when logging in to AWS and running the returned docker command is sorted.
#[string]$region = "eu-west-2"

function BuildImages(){
    Write-Host("Building images for version $version");
    Write-Host("===============");
    & docker image build -t $imageBase/api:$version -t $imageBase/api:latest --force-rm -f ./build/api/Dockerfile .
    & docker image build -t $imageBase/worker:$version -t $imageBase/worker:latest --force-rm -f ./build/worker/Dockerfile .
    & docker image build -t $imageBase/migrations:$version -t $imageBase/migrations:latest --force-rm -f ./build/migrations/Dockerfile .
}

function PushImages(){
    # Todo: Login to aws and docker.
    # $(aws ecr get-login --region $region --no-include-email)
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