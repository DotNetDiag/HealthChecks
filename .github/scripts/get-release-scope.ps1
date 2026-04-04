param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [string[]]$AdditionalProjectPaths = @(),

    [string]$RepositoryRoot = (Get-Location).Path,

    [string]$TagName = $env:GITHUB_REF_NAME,

    [string]$GitHubOutputPath = $env:GITHUB_OUTPUT
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$defaultGlobalFiles = @(
    'Directory.Build.props',
    'Directory.Build.targets',
    'Directory.Packages.props',
    'src/Directory.Build.props',
    'src/ApiMarker.cs',
    'src/CallerArgumentExpressionAttribute.cs'
)

function ConvertTo-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $normalized = $Path.Replace('\\', '/').Replace('\', '/')
    if ($normalized.StartsWith('./')) {
        $normalized = $normalized.Substring(2)
    }

    return $normalized.TrimStart('/')
}

function ConvertTo-RepoRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$FullPath,
        [Parameter(Mandatory = $true)][string]$RepoRoot
    )

    $resolvedFullPath = (Resolve-Path -LiteralPath $FullPath).Path.Replace('\', '/')
    $resolvedRepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path.Replace('\', '/').TrimEnd('/')

    if (-not $resolvedFullPath.StartsWith($resolvedRepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$FullPath' is outside repository root '$RepoRoot'."
    }

    return $resolvedFullPath.Substring($resolvedRepoRoot.Length).TrimStart('/')
}

function Get-ReferencedProjectPaths {
    param(
        [Parameter(Mandatory = $true)][string]$RootProjectPath,
        [Parameter(Mandatory = $true)][string]$RepoRoot
    )

    $visited = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $references = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    function VisitProject {
        param([Parameter(Mandatory = $true)][string]$CurrentProjectPath)

        $resolvedProjectPath = (Resolve-Path -LiteralPath $CurrentProjectPath).Path
        if (-not $visited.Add($resolvedProjectPath)) {
            return
        }

        $null = $references.Add((ConvertTo-RepoRelativePath -FullPath $resolvedProjectPath -RepoRoot $RepoRoot))

        $projectXml = [xml](Get-Content -LiteralPath $resolvedProjectPath -Raw)
        $projectDirectory = Split-Path -Path $resolvedProjectPath -Parent
        $projectReferenceNodes = Select-Xml -Xml $projectXml -XPath '//ProjectReference'

        foreach ($projectReferenceNode in $projectReferenceNodes) {
            $includePath = $projectReferenceNode.Node.Include
            if ([string]::IsNullOrWhiteSpace($includePath)) {
                continue
            }

            $combinedPath = [System.IO.Path]::GetFullPath((Join-Path -Path $projectDirectory -ChildPath $includePath))
            if (-not (Test-Path -LiteralPath $combinedPath)) {
                continue
            }

            VisitProject -CurrentProjectPath $combinedPath
        }
    }

    VisitProject -CurrentProjectPath $RootProjectPath
    return @($references)
}

function Write-OutputVariable {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [AllowEmptyString()]
        [Parameter(Mandatory = $true)][string]$Value,
        [string]$OutputPath
    )

    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        return
    }

    Add-Content -LiteralPath $OutputPath -Value "$Name=$Value"
}

$results = [ordered]@{
    is_tag = 'false'
    should_run = 'true'
    package_version = ''
    previous_tag = ''
    release_mode = 'ci'
    release_reason = 'Branch, pull request, or manual run; execute the normal CI flow.'
    tracked_paths = ''
}

$refType = $env:GITHUB_REF_TYPE
if ($refType -ne 'tag') {
    $results.tracked_paths = (ConvertTo-RepoPath -Path $ProjectPath)
}
else {
    if ([string]::IsNullOrWhiteSpace($TagName)) {
        throw 'Tag build detected, but GITHUB_REF_NAME is empty.'
    }

    if ($TagName -notmatch '^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$') {
        throw "Tag '$TagName' must match the format vA.B.C."
    }

    $results.is_tag = 'true'
    $results.package_version = $TagName.Substring(1)
    $currentMajorMinor = "$($Matches.major).$($Matches.minor)"

    $previousTag = @(
        git tag --merged HEAD --list 'v*' --sort=-v:refname |
        Where-Object { $_ -ne $TagName }
    ) | Select-Object -First 1

    $results.previous_tag = $previousTag

    $trackedProjectFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $trackedDirectories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($candidateProjectPath in @($ProjectPath) + $AdditionalProjectPaths) {
        if ([string]::IsNullOrWhiteSpace($candidateProjectPath)) {
            continue
        }

        foreach ($referencedProjectPath in Get-ReferencedProjectPaths -RootProjectPath $candidateProjectPath -RepoRoot $RepositoryRoot) {
            $normalizedProjectPath = ConvertTo-RepoPath -Path $referencedProjectPath
            $null = $trackedProjectFiles.Add($normalizedProjectPath)

            $projectDirectory = Split-Path -Path $normalizedProjectPath -Parent
            if (-not [string]::IsNullOrWhiteSpace($projectDirectory)) {
                $null = $trackedDirectories.Add(($projectDirectory.Replace('\', '/') + '/'))
            }
        }
    }

    $results.tracked_paths = ((@($trackedProjectFiles) | Sort-Object) -join ',')

    $changedFiles = if ([string]::IsNullOrWhiteSpace($previousTag)) {
        @(git ls-files)
    }
    else {
        @(git diff --name-only $previousTag HEAD)
    }

    $buildAll = $false
    $releaseMode = 'incremental'
    $releaseReason = 'Patch version changed; project or dependency files must change to build and publish.'

    if ([string]::IsNullOrWhiteSpace($previousTag)) {
        $buildAll = $true
        $releaseMode = 'full'
        $releaseReason = 'No previous version tag was found, so every project should build and publish.'
    }
    elseif ($previousTag -notmatch '^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$') {
        $buildAll = $true
        $releaseMode = 'full'
        $releaseReason = "Previous tag '$previousTag' does not match vA.B.C, so every project should build and publish."
    }
    else {
        $previousMajorMinor = "$($Matches.major).$($Matches.minor)"
        if ($currentMajorMinor -ne $previousMajorMinor) {
            $buildAll = $true
            $releaseMode = 'full'
            $releaseReason = "Major or minor version changed from $previousTag to $TagName, so every project should build and publish."
        }
    }

    if (-not $buildAll) {
        foreach ($globalFile in $defaultGlobalFiles) {
            if ($changedFiles -contains $globalFile) {
                $buildAll = $true
                $releaseMode = 'full'
                $releaseReason = "Shared build or dependency file '$globalFile' changed, so every project should build and publish."
                break
            }
        }
    }

    $shouldRun = $buildAll
    if (-not $shouldRun) {
        foreach ($changedFile in $changedFiles) {
            $normalizedChangedFile = ConvertTo-RepoPath -Path $changedFile

            if ($trackedProjectFiles.Contains($normalizedChangedFile)) {
                $shouldRun = $true
                $releaseReason = "Project or referenced project file '$normalizedChangedFile' changed since $previousTag."
                break
            }

            foreach ($trackedDirectory in $trackedDirectories) {
                if ($normalizedChangedFile.StartsWith($trackedDirectory, [System.StringComparison]::OrdinalIgnoreCase)) {
                    $shouldRun = $true
                    $releaseReason = "Project or referenced project content under '$trackedDirectory' changed since $previousTag."
                    break
                }
            }

            if ($shouldRun) {
                break
            }
        }
    }

    $results.should_run = if ($shouldRun) { 'true' } else { 'false' }
    $results.release_mode = $releaseMode
    $results.release_reason = $releaseReason
}

foreach ($entry in $results.GetEnumerator()) {
    Write-OutputVariable -Name $entry.Key -Value ([string]$entry.Value) -OutputPath $GitHubOutputPath
}

$results | ConvertTo-Json -Compress
