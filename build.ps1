param (
    [string]$version = "0.0.0",
    [string]$imageBase = "score-sys",
    [string]$environment = "dev",
    [string]$awsAccountId = "PASS-IN-ID"
)

[string]$region = "eu-west-2"

function BuildImages(){
    Write-Host("Building images for version $version");
    Write-Host("===============");
    & docker image build -t $imageBase/api:$version --force-rm -f ./build/api/Dockerfile .
    & docker image build -t $imageBase/worker:$version --force-rm -f ./build/worker/Dockerfile .
}

function PushImages(){
    Write-Host("Attempting to log in to docker via AWS...")
    aws ecr get-login-password --region ${region} | docker login --username AWS --password-stdin "${awsAccountId}.dkr.ecr.${region}.amazonaws.com"
    
    Write-Host("Pushing images");
    Write-Host("==============");

    Write-Host("Versioned images:");
    & docker push $imageBase/api:$version
    & docker push $imageBase/worker:$version
}

BuildImages
Write-Host("Images built");

if($imageBase -ne "score-sys"){
    PushImages
    Write-Host("Images pushed");
}