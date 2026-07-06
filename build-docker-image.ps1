Param(
    [Parameter(Mandatory = $false)][bool]$PublishToGitHubContainerRegistry = $false,
    [Parameter(Mandatory = $false)][string]$Tag,
    [Parameter(Mandatory = $false)][string]$ImageRepository = "ghcr.io/dotnetdiag/healthchecksui"
)

Set-StrictMode -Version Latest

function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage = "Command failed: $cmd"
    )
    & $cmd
    if ($LASTEXITCODE -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

function Get-DefaultTag {
    $propsPath = Join-Path $PSScriptRoot "Directory.Build.props"
    $version = Select-Xml -Path $propsPath -XPath "/Project/PropertyGroup/VersionPrefix" | Select-Object -First 1

    if ($null -eq $version) {
        throw "Unable to find VersionPrefix in $propsPath. Pass -Tag explicitly."
    }

    return $version.Node.InnerText
}

if ([string]::IsNullOrWhiteSpace($Tag)) {
    $Tag = Get-DefaultTag
}

#Building docker image

Write-Host "Building docker image with tag: $Tag"
Write-Host "Image repository: $ImageRepository"
Write-Host "Publish to GitHub Container Registry: $PublishToGitHubContainerRegistry"

Exec { & docker build . -f "$PSScriptRoot/build/docker-images/HealthChecks.UI.Image/Dockerfile" -t "${ImageRepository}:$Tag" }
Exec { & docker tag "${ImageRepository}:$Tag" "${ImageRepository}:latest" }

Write-Host "Created docker image ${ImageRepository}:$Tag. You can execute this image using docker run"
Write-Host "Sample: docker run --name ui -p 5000:8080 -e 'HealthChecksUI:HealthChecks:0:Name=httpBasic' -e 'HealthChecksUI:HealthChecks:0:Uri=http://www.google.es' -d ${ImageRepository}:$Tag"

#Publish it

if ($PublishToGitHubContainerRegistry) {
    Exec { & docker push "${ImageRepository}:$Tag" }
    Exec { & docker push "${ImageRepository}:latest" }
}
