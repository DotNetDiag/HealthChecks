Param(
    [Parameter(Mandatory = $false)][bool]$PublishToGitHubContainerRegistry = $false,
    [Parameter(Mandatory = $false)][string]$Tag,
    [Parameter(Mandatory = $false)][string]$ImageRepository = "ghcr.io/dotnetdiag/healthchecksui-k8s-operator",
    [Parameter(Mandatory = $false)][string]$TargetFramework = "net10.0",
    [Parameter(Mandatory = $false)][string]$Platforms,
    [Parameter(Mandatory = $false)][string]$ImageSource = "https://github.com/DotNetDiag/HealthChecks",
    [Parameter(Mandatory = $false)][string]$ImageRevision,
    [Parameter(Mandatory = $false)][bool]$Pull = $false
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

function Get-DefaultRevision {
    $revision = & git -C $PSScriptRoot rev-parse --short=12 HEAD 2>$null

    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($revision)) {
        return $revision.Trim()
    }

    return "unknown"
}

function Assert-GitHubContainerRegistryRepository {
    param(
        [Parameter(Mandatory = 1)][string]$Repository
    )

    if (-not $Repository.StartsWith("ghcr.io/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Publishing the UI operator image is restricted to GitHub Container Registry. Use an ImageRepository that starts with 'ghcr.io/' or omit -PublishToGitHubContainerRegistry to build/load locally."
    }
}

if ([string]::IsNullOrWhiteSpace($Tag)) {
    $Tag = Get-DefaultTag
}

if ([string]::IsNullOrWhiteSpace($ImageRevision)) {
    $ImageRevision = Get-DefaultRevision
}

if ($PublishToGitHubContainerRegistry) {
    Assert-GitHubContainerRegistryRepository $ImageRepository
}

if ([string]::IsNullOrWhiteSpace($Platforms)) {
    if ($PublishToGitHubContainerRegistry) {
        $Platforms = "linux/amd64,linux/arm64"
    }
    else {
        $Platforms = "linux/amd64"
    }
}

if (-not $PublishToGitHubContainerRegistry -and $Platforms.Split(",").Count -gt 1) {
    throw "Multiple platforms require -PublishToGitHubContainerRegistry `$true because Docker cannot load a multi-platform image into the local image store."
}

#Building docker image

Write-Host "Building k8s operator docker image with tag: $Tag"
Write-Host "Image repository: $ImageRepository"
Write-Host "Image revision: $ImageRevision"
Write-Host "Platforms: $Platforms"
Write-Host "Target framework: $TargetFramework"
Write-Host "Pull latest base images: $Pull"
Write-Host "Publishing to GitHub Container Registry: $PublishToGitHubContainerRegistry"

$dockerBuildArgs = @(
    "buildx",
    "build",
    ".",
    "-f",
    "$PSScriptRoot/src/HealthChecks.UI.K8s.Operator/Dockerfile",
    "--platform",
    $Platforms,
    "--build-arg",
    "TARGET_FRAMEWORK=$TargetFramework",
    "--build-arg",
    "IMAGE_SOURCE=$ImageSource",
    "--build-arg",
    "IMAGE_REVISION=$ImageRevision",
    "--build-arg",
    "IMAGE_VERSION=$Tag",
    "-t",
    "${ImageRepository}:$Tag",
    "-t",
    "${ImageRepository}:latest"
)

if ($Pull) {
    $dockerBuildArgs += "--pull"
}

if ($PublishToGitHubContainerRegistry) {
    $dockerBuildArgs += "--push"
}
else {
    $dockerBuildArgs += "--load"
}

Exec { & docker @dockerBuildArgs }

Write-Host "Created docker image ${ImageRepository}:$Tag. You can execute this image using docker run"
