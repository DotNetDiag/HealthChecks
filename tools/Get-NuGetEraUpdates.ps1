param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$PackagesPropsPath = 'Directory.Packages.props',
    [string]$NuGetFlatContainer = 'https://api.nuget.org/v3-flatcontainer',
    [string[]]$PackageId,
    [switch]$IncludePrerelease,
    [switch]$OnlyOutdated,
    [switch]$AsJson,
    [int]$LegacyEraMajor = 8,
    [string]$CachePath = (Join-Path ([System.IO.Path]::GetTempPath()) 'healthchecks-nuget-era-cache')
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$eraBoundPackagePatterns = @(
    'Microsoft.AspNetCore.*',
    'Microsoft.Data.Sqlite',
    'Microsoft.EntityFrameworkCore*',
    'Microsoft.Extensions.*',
    'Npgsql.EntityFrameworkCore.*',
    'Pomelo.EntityFrameworkCore.*',
    'System.*'
)

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Join-RepoPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $RepositoryRoot $Path
}

function Read-XmlFile {
    param([string]$Path)

    [xml](Get-Content -LiteralPath $Path -Raw)
}

function Get-AttributeValue {
    param(
        [System.Xml.XmlElement]$Node,
        [string]$Name
    )

    if ($Node.HasAttribute($Name)) {
        return $Node.GetAttribute($Name)
    }

    return $null
}

function Expand-MSBuildValue {
    param(
        [string]$Value,
        [hashtable]$Properties
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $Value
    }

    $expanded = $Value
    for ($i = 0; $i -lt 20; $i++) {
        $next = [regex]::Replace(
            $expanded,
            '\$\(([^\)]+)\)',
            {
                param($match)

                $name = $match.Groups[1].Value
                if ($Properties.ContainsKey($name)) {
                    return [string]$Properties[$name]
                }

                return $match.Value
            })

        if ($next -eq $expanded) {
            break
        }

        $expanded = $next
    }

    return $expanded
}

function Add-PropertiesFromFile {
    param(
        [string]$Path,
        [hashtable]$Properties
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $xml = Read-XmlFile $Path
    foreach ($propertyGroup in @($xml.SelectNodes('/Project/PropertyGroup'))) {
        foreach ($child in @($propertyGroup.ChildNodes)) {
            if ($child.NodeType -ne [System.Xml.XmlNodeType]::Element) {
                continue
            }

            if ([string]::IsNullOrWhiteSpace($child.InnerText)) {
                continue
            }

            $Properties[$child.Name] = Expand-MSBuildValue $child.InnerText.Trim() $Properties
        }
    }
}

function Get-DirectoryBuildPropsChain {
    param([string]$ProjectPath)

    $root = (Resolve-Path $RepositoryRoot).Path.TrimEnd('\')
    $projectDirectory = (Split-Path -Parent (Resolve-Path $ProjectPath).Path).TrimEnd('\')
    $paths = New-Object System.Collections.Generic.List[string]
    $current = $projectDirectory

    while ($current.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        $candidate = Join-Path $current 'Directory.Build.props'
        if (Test-Path -LiteralPath $candidate) {
            $paths.Add((Resolve-Path $candidate).Path)
        }

        if ($current -ieq $root) {
            break
        }

        $current = (Split-Path -Parent $current).TrimEnd('\')
    }

    [array]::Reverse($paths)
    return $paths
}

function Get-ProjectInfo {
    param([string]$ProjectPath)

    $properties = @{}
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    $properties['MSBuildProjectName'] = $projectName

    foreach ($propsPath in Get-DirectoryBuildPropsChain $ProjectPath) {
        Add-PropertiesFromFile $propsPath $properties
    }

    $projectXml = Read-XmlFile $ProjectPath
    foreach ($propertyGroup in @($projectXml.SelectNodes('/Project/PropertyGroup'))) {
        foreach ($child in @($propertyGroup.ChildNodes)) {
            if ($child.NodeType -ne [System.Xml.XmlNodeType]::Element) {
                continue
            }

            if ([string]::IsNullOrWhiteSpace($child.InnerText)) {
                continue
            }

            $properties[$child.Name] = Expand-MSBuildValue $child.InnerText.Trim() $properties
        }
    }

    $targetFrameworkValue = $null
    if ($properties.ContainsKey('TargetFrameworks')) {
        $targetFrameworkValue = [string]$properties['TargetFrameworks']
    }
    elseif ($properties.ContainsKey('TargetFramework')) {
        $targetFrameworkValue = [string]$properties['TargetFramework']
    }

    $targetFrameworks = @()
    if (-not [string]::IsNullOrWhiteSpace($targetFrameworkValue)) {
        $expanded = Expand-MSBuildValue $targetFrameworkValue $properties
        $targetFrameworks = @($expanded -split ';' | Where-Object { $_ -and $_ -notmatch '\$\(' } | Sort-Object -Unique)
    }

    [pscustomobject]@{
        Path = (Resolve-Path $ProjectPath).Path
        RelativePath = (Resolve-Path $ProjectPath).Path.Substring((Resolve-Path $RepositoryRoot).Path.Length + 1)
        Name = $projectName
        TargetFrameworks = $targetFrameworks
        Properties = $properties
        Xml = $projectXml
    }
}

function Test-TargetFrameworkCondition {
    param(
        [string]$Condition,
        [string]$TargetFramework
    )

    if ([string]::IsNullOrWhiteSpace($Condition)) {
        return $true
    }

    $parts = [regex]::Split($Condition.Trim(), '\s+(?i:AND)\s+')
    foreach ($part in $parts) {
        $trimmed = $part.Trim()
        if ($trimmed -match "^\(?\s*['""]?\$\(TargetFramework\)['""]?\s*==\s*['""]([^'""]+)['""]\s*\)?$") {
            if ($TargetFramework -ne $Matches[1]) {
                return $false
            }

            continue
        }

        if ($trimmed -match "^\(?\s*['""]?\$\(TargetFramework\)['""]?\s*!=\s*['""]([^'""]+)['""]\s*\)?$") {
            if ($TargetFramework -eq $Matches[1]) {
                return $false
            }

            continue
        }

        return $null
    }

    return $true
}

function Get-CombinedCondition {
    param([System.Xml.XmlElement]$Node)

    $conditions = New-Object System.Collections.Generic.List[string]
    $parent = $Node.ParentNode
    if ($parent -is [System.Xml.XmlElement]) {
        $parentCondition = Get-AttributeValue $parent 'Condition'
        if (-not [string]::IsNullOrWhiteSpace($parentCondition)) {
            $conditions.Add($parentCondition)
        }
    }

    $nodeCondition = Get-AttributeValue $Node 'Condition'
    if (-not [string]::IsNullOrWhiteSpace($nodeCondition)) {
        $conditions.Add($nodeCondition)
    }

    return ($conditions -join ' AND ')
}

function Get-PackageReferenceId {
    param([System.Xml.XmlElement]$Node)

    foreach ($attributeName in @('Include', 'Update')) {
        $value = Get-AttributeValue $Node $attributeName
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    return $null
}

function Get-PackageReferenceVersionSpec {
    param([System.Xml.XmlElement]$Node)

    foreach ($attributeName in @('VersionOverride', 'PackageOverride', 'Version')) {
        $value = Get-AttributeValue $Node $attributeName
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return [pscustomobject]@{
                Version = $value
                Attribute = $attributeName
            }
        }
    }

    return $null
}

function Add-ToSet {
    param(
        [hashtable]$Map,
        [string]$Key,
        [string]$Value
    )

    if (-not $Map.ContainsKey($Key)) {
        $Map[$Key] = New-Object 'System.Collections.Generic.HashSet[string]'
    }

    [void]$Map[$Key].Add($Value)
}

function ConvertTo-Version {
    param([string]$Version)

    $core = ($Version -replace '\+.*$', '') -replace '-.*$', ''
    [version]$core
}

function Get-VersionMajor {
    param([string]$Version)

    (ConvertTo-Version $Version).Major
}

function Test-StableVersion {
    param([string]$Version)

    return $Version -notmatch '-'
}

function Test-PackageEraBound {
    param([string]$Id)

    foreach ($pattern in $eraBoundPackagePatterns) {
        if ($Id -like $pattern) {
            return $true
        }
    }

    return $false
}

function Get-TargetEraMajor {
    param([string]$TargetFramework)

    if ($TargetFramework -match '^net(\d+)\.(\d+)') {
        return [int]$Matches[1]
    }

    if ($TargetFramework -match '^netstandard') {
        return $LegacyEraMajor
    }

    return $null
}

function Get-NuGetVersions {
    param([string]$Id)

    $versionsDirectory = Join-Path $CachePath 'versions'
    New-Item -ItemType Directory -Path $versionsDirectory -Force | Out-Null

    $cacheFile = Join-Path $versionsDirectory "$($Id.ToLowerInvariant()).json"
    if (-not (Test-Path -LiteralPath $cacheFile)) {
        $url = "$($NuGetFlatContainer.TrimEnd('/'))/$($Id.ToLowerInvariant())/index.json"
        Invoke-RestMethod -Uri $url -OutFile $cacheFile -TimeoutSec 60
    }

    $json = Get-Content -LiteralPath $cacheFile -Raw | ConvertFrom-Json
    return @($json.versions)
}

function Get-NuGetPackagePath {
    param(
        [string]$Id,
        [string]$Version
    )

    $packageDirectory = Join-Path (Join-Path $CachePath 'packages') $Id.ToLowerInvariant()
    New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null

    $packagePath = Join-Path $packageDirectory "$($Id.ToLowerInvariant()).$Version.nupkg"
    if (-not (Test-Path -LiteralPath $packagePath)) {
        $url = "$($NuGetFlatContainer.TrimEnd('/'))/$($Id.ToLowerInvariant())/$Version/$($Id.ToLowerInvariant()).$Version.nupkg"
        Invoke-WebRequest -Uri $url -OutFile $packagePath -UseBasicParsing -TimeoutSec 120
    }

    return $packagePath
}

function Get-PackageAssetFrameworks {
    param(
        [string]$Id,
        [string]$Version
    )

    $cacheDirectory = Join-Path $CachePath 'assets'
    New-Item -ItemType Directory -Path $cacheDirectory -Force | Out-Null
    $cacheFile = Join-Path $cacheDirectory "$($Id.ToLowerInvariant()).$Version.json"
    if (Test-Path -LiteralPath $cacheFile) {
        return @((Get-Content -LiteralPath $cacheFile -Raw | ConvertFrom-Json).frameworks)
    }

    $packagePath = Get-NuGetPackagePath $Id $Version
    $frameworks = New-Object 'System.Collections.Generic.HashSet[string]'
    $hasAnyAsset = $false
    $zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        foreach ($entry in $zip.Entries) {
            $name = $entry.FullName
            if ($name -match '^(lib|ref|build|buildTransitive)/([^/]+)/') {
                $hasAnyAsset = $true
                [void]$frameworks.Add($Matches[2])
                continue
            }

            if ($name -match '^(analyzers|contentFiles|tools)/') {
                $hasAnyAsset = $true
            }
        }
    }
    finally {
        $zip.Dispose()
    }

    $payload = [pscustomobject]@{
        hasAnyAsset = $hasAnyAsset
        frameworks = @($frameworks | Sort-Object)
    }
    $payload | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $cacheFile -Encoding UTF8

    return @($payload.frameworks)
}

function ConvertTo-TfmInfo {
    param([string]$TargetFramework)

    $tfm = ($TargetFramework -replace '-.*$', '').ToLowerInvariant()
    if ($tfm -match '^netstandard(\d+)\.(\d+)$') {
        return [pscustomobject]@{ Family = 'netstandard'; Major = [int]$Matches[1]; Minor = [int]$Matches[2] }
    }

    if ($tfm -match '^netcoreapp(\d+)\.(\d+)$') {
        return [pscustomobject]@{ Family = 'netcoreapp'; Major = [int]$Matches[1]; Minor = [int]$Matches[2] }
    }

    if ($tfm -match '^net(\d+)\.(\d+)$') {
        return [pscustomobject]@{ Family = 'net'; Major = [int]$Matches[1]; Minor = [int]$Matches[2] }
    }

    if ($tfm -match '^net(\d+)$') {
        return [pscustomobject]@{ Family = 'net'; Major = [int]$Matches[1]; Minor = 0 }
    }

    return $null
}

function Compare-TfmVersion {
    param($Left, $Right)

    if ($Left.Major -ne $Right.Major) {
        return $Left.Major.CompareTo($Right.Major)
    }

    return $Left.Minor.CompareTo($Right.Minor)
}

function Test-AssetCompatible {
    param(
        [string]$AssetFramework,
        [string]$TargetFramework
    )

    $asset = ConvertTo-TfmInfo $AssetFramework
    $target = ConvertTo-TfmInfo $TargetFramework
    if ($null -eq $asset -or $null -eq $target) {
        return $false
    }

    if ($target.Family -eq 'net') {
        if ($asset.Family -eq 'net') {
            return (Compare-TfmVersion $asset $target) -le 0
        }

        if ($asset.Family -eq 'netstandard') {
            return (Compare-TfmVersion $asset ([pscustomobject]@{ Major = 2; Minor = 1 })) -le 0
        }

        if ($asset.Family -eq 'netcoreapp') {
            return $asset.Major -le 3
        }
    }

    if ($target.Family -eq 'netstandard') {
        return $asset.Family -eq 'netstandard' -and (Compare-TfmVersion $asset $target) -le 0
    }

    if ($target.Family -eq 'netcoreapp') {
        if ($asset.Family -eq 'netcoreapp') {
            return (Compare-TfmVersion $asset $target) -le 0
        }

        if ($asset.Family -eq 'netstandard') {
            return (Compare-TfmVersion $asset ([pscustomobject]@{ Major = 2; Minor = 1 })) -le 0
        }
    }

    return $false
}

function Test-PackageVersionCompatible {
    param(
        [string]$Id,
        [string]$Version,
        [string]$TargetFramework
    )

    $assetFrameworks = @(Get-PackageAssetFrameworks $Id $Version)
    if ($assetFrameworks.Count -eq 0) {
        return $true
    }

    foreach ($assetFramework in $assetFrameworks) {
        if (Test-AssetCompatible $assetFramework $TargetFramework) {
            return $true
        }
    }

    return $false
}

function Get-VersionRange {
    param([string]$Spec)

    if ([string]::IsNullOrWhiteSpace($Spec)) {
        return $null
    }

    if ($Spec -notmatch '^([\[\(])([^,]*),([^\]\)]*)([\]\)])$') {
        return $null
    }

    [pscustomobject]@{
        Min = $Matches[2].Trim()
        Max = $Matches[3].Trim()
        IncludeMin = $Matches[1] -eq '['
        IncludeMax = $Matches[4] -eq ']'
    }
}

function Test-VersionInRange {
    param(
        [string]$Version,
        $Range
    )

    $parsed = ConvertTo-Version $Version
    if (-not [string]::IsNullOrWhiteSpace($Range.Min)) {
        $compareMin = $parsed.CompareTo((ConvertTo-Version $Range.Min))
        if ($compareMin -lt 0 -or ($compareMin -eq 0 -and -not $Range.IncludeMin)) {
            return $false
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Range.Max)) {
        $compareMax = $parsed.CompareTo((ConvertTo-Version $Range.Max))
        if ($compareMax -gt 0 -or ($compareMax -eq 0 -and -not $Range.IncludeMax)) {
            return $false
        }
    }

    return $true
}

function Test-VersionSpecSatisfied {
    param(
        [string]$Spec,
        [string]$RecommendedVersion
    )

    if ([string]::IsNullOrWhiteSpace($Spec) -or [string]::IsNullOrWhiteSpace($RecommendedVersion)) {
        return $false
    }

    $range = Get-VersionRange $Spec
    if ($null -ne $range) {
        return Test-VersionInRange $RecommendedVersion $range
    }

    if ($Spec.EndsWith('.*', [System.StringComparison]::Ordinal)) {
        $prefix = $Spec.Substring(0, $Spec.Length - 1)
        return $RecommendedVersion.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)
    }

    try {
        return (ConvertTo-Version $Spec).CompareTo((ConvertTo-Version $RecommendedVersion)) -eq 0
    }
    catch {
        return $Spec -ieq $RecommendedVersion
    }
}

function Find-CompatibleVersion {
    param(
        [string]$Id,
        [string]$TargetFramework,
        [string]$CurrentSpec
    )

    $policy = New-Object System.Collections.Generic.List[string]
    $versions = @(Get-NuGetVersions $Id)

    if (-not $IncludePrerelease) {
        $versions = @($versions | Where-Object { Test-StableVersion $_ })
        $policy.Add('stable')
    }
    else {
        $policy.Add('including-prerelease')
    }

    $range = Get-VersionRange $CurrentSpec
    if ($null -ne $range) {
        $versions = @($versions | Where-Object { Test-VersionInRange $_ $range })
        $policy.Add("declared-range:$CurrentSpec")
    }

    $eraMajor = Get-TargetEraMajor $TargetFramework
    if ((Test-PackageEraBound $Id) -and $null -ne $eraMajor) {
        $versions = @($versions | Where-Object { (Get-VersionMajor $_) -le $eraMajor })
        $policy.Add("era-bound:<=$eraMajor.x")
    }
    else {
        $policy.Add('tfm-compatible')
    }

    $versions = @($versions | Sort-Object { ConvertTo-Version $_ } -Descending)
    foreach ($version in $versions) {
        if (Test-PackageVersionCompatible $Id $version $TargetFramework) {
            return [pscustomobject]@{
                Version = $version
                Policy = ($policy -join ';')
                Error = $null
            }
        }
    }

    return [pscustomobject]@{
        Version = $null
        Policy = ($policy -join ';')
        Error = 'No compatible version found'
    }
}

$packagesPropsFullPath = Join-RepoPath $PackagesPropsPath
$projectInfos = @(Get-ChildItem -LiteralPath $RepositoryRoot -Recurse -Filter '*.csproj' | ForEach-Object { Get-ProjectInfo $_.FullName })
$allTargetFrameworks = @($projectInfos.TargetFrameworks | ForEach-Object { $_ } | Sort-Object -Unique)
$srcTargetFrameworks = @($projectInfos | Where-Object { $_.RelativePath -like 'src\*' } | ForEach-Object { $_.TargetFrameworks } | Sort-Object -Unique)
$testTargetFrameworks = @($projectInfos | Where-Object { $_.RelativePath -like 'test\*' } | ForEach-Object { $_.TargetFrameworks } | Sort-Object -Unique)

$usageMap = @{}
$overrideRows = New-Object System.Collections.Generic.List[object]

function Add-PackageReferencesFromXml {
    param(
        [string]$Path,
        [string[]]$TargetFrameworks,
        [string]$Scope,
        [string]$RelativePath
    )

    $xml = Read-XmlFile $Path
    foreach ($node in @($xml.SelectNodes('//PackageReference'))) {
        if ($node.HasAttribute('Remove')) {
            continue
        }

        $id = Get-PackageReferenceId $node
        if ([string]::IsNullOrWhiteSpace($id)) {
            continue
        }

        foreach ($tfm in $TargetFrameworks) {
            $condition = Get-CombinedCondition $node
            $conditionResult = Test-TargetFrameworkCondition $condition $tfm
            if ($conditionResult -eq $false) {
                continue
            }

            Add-ToSet $usageMap $id $tfm
            $versionSpec = Get-PackageReferenceVersionSpec $node
            if ($null -ne $versionSpec) {
                $overrideRows.Add([pscustomobject]@{
                    Scope = $Scope
                    PackageId = $id
                    TargetFramework = $tfm
                    CurrentSpec = $versionSpec.Version
                    CurrentSource = $versionSpec.Attribute
                    File = $RelativePath
                    Condition = $condition
                    ConditionReviewNeeded = $conditionResult -eq $null
                })
            }
        }
    }
}

foreach ($projectInfo in $projectInfos) {
    Add-PackageReferencesFromXml $projectInfo.Path $projectInfo.TargetFrameworks 'ProjectOverride' $projectInfo.RelativePath
}

foreach ($buildFile in @('Directory.Build.props', 'Directory.Build.targets')) {
    $fullPath = Join-RepoPath $buildFile
    if (Test-Path -LiteralPath $fullPath) {
        Add-PackageReferencesFromXml $fullPath $allTargetFrameworks 'ImportedOverride' $buildFile
    }
}

$srcBuildProps = Join-RepoPath 'src\Directory.Build.props'
if (Test-Path -LiteralPath $srcBuildProps) {
    Add-PackageReferencesFromXml $srcBuildProps $srcTargetFrameworks 'ImportedOverride' 'src\Directory.Build.props'
}

$testBuildProps = Join-RepoPath 'test\Directory.Build.props'
if (Test-Path -LiteralPath $testBuildProps) {
    Add-PackageReferencesFromXml $testBuildProps $testTargetFrameworks 'ImportedOverride' 'test\Directory.Build.props'
}

$packagesXml = Read-XmlFile $packagesPropsFullPath
$centralRows = New-Object System.Collections.Generic.List[object]
foreach ($node in @($packagesXml.SelectNodes('//PackageVersion'))) {
    $id = Get-AttributeValue $node 'Include'
    if ([string]::IsNullOrWhiteSpace($id)) {
        continue
    }

    $currentSpec = Get-AttributeValue $node 'Version'
    $condition = Get-CombinedCondition $node
    $targetFrameworks = $allTargetFrameworks
    if ($usageMap.ContainsKey($id)) {
        $targetFrameworks = @($usageMap[$id] | Sort-Object)
    }

    foreach ($tfm in $targetFrameworks) {
        $conditionResult = Test-TargetFrameworkCondition $condition $tfm
        if ($conditionResult -eq $false) {
            continue
        }

        $centralRows.Add([pscustomobject]@{
            Scope = 'Central'
            PackageId = $id
            TargetFramework = $tfm
            CurrentSpec = $currentSpec
            CurrentSource = 'PackageVersion'
            File = $PackagesPropsPath
            Condition = $condition
            ConditionReviewNeeded = $conditionResult -eq $null
        })
    }
}

$rows = @($centralRows + $overrideRows)
if ($PackageId) {
    $packageSet = @{}
    foreach ($id in $PackageId) {
        $packageSet[$id] = $true
    }

    $rows = @($rows | Where-Object { $packageSet.ContainsKey($_.PackageId) })
}

$recommendationCache = @{}
$results = foreach ($row in $rows) {
    $cacheKey = "$($row.PackageId)|$($row.TargetFramework)|$($row.CurrentSpec)|$IncludePrerelease|$LegacyEraMajor"
    if (-not $recommendationCache.ContainsKey($cacheKey)) {
        try {
            $recommendationCache[$cacheKey] = Find-CompatibleVersion $row.PackageId $row.TargetFramework $row.CurrentSpec
        }
        catch {
            $recommendationCache[$cacheKey] = [pscustomobject]@{
                Version = $null
                Policy = 'error'
                Error = $_.Exception.Message
            }
        }
    }

    $recommendation = $recommendationCache[$cacheKey]
    $needsChange = $false
    if (-not [string]::IsNullOrWhiteSpace($recommendation.Version)) {
        $needsChange = -not (Test-VersionSpecSatisfied $row.CurrentSpec $recommendation.Version)
    }

    [pscustomobject]@{
        PackageId = $row.PackageId
        TargetFramework = $row.TargetFramework
        Scope = $row.Scope
        Current = $row.CurrentSpec
        Recommended = $recommendation.Version
        NeedsChange = $needsChange
        Policy = $recommendation.Policy
        File = $row.File
        CurrentSource = $row.CurrentSource
        ConditionReviewNeeded = $row.ConditionReviewNeeded
        Error = $recommendation.Error
    }
}

if ($OnlyOutdated) {
    $results = @($results | Where-Object { $_.NeedsChange -or $_.ConditionReviewNeeded -or $_.Error })
}

$results = @($results | Sort-Object PackageId, TargetFramework, Scope, File -Unique)

if ($AsJson) {
    $results | ConvertTo-Json -Depth 8
}
else {
    $results | Format-Table PackageId, TargetFramework, Scope, Current, Recommended, NeedsChange, Policy, File -AutoSize
}
