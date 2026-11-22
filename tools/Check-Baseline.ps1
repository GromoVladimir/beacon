# Статическая проверка T01. Не заменяет импорт и проверку Console в Unity.
[CmdletBinding()]
param([string] $ProjectPath)

$ErrorActionPreference = 'Stop'
if (-not $ProjectPath) { $ProjectPath = Split-Path $PSScriptRoot -Parent }
$root = (Resolve-Path -LiteralPath $ProjectPath).Path
$issues = [System.Collections.Generic.List[string]]::new()
function Read-ProjectFile([string] $relative) {
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing input: $relative"
    }
    return [IO.File]::ReadAllText($path)
}

$version = Read-ProjectFile 'ProjectSettings/ProjectVersion.txt'
if ($version -notmatch '(?m)^m_EditorVersion: 6000\.6\.10f1\s*$') {
    $issues.Add('Editor version must be 6000.6.10f1')
}
if ((Read-ProjectFile 'ProjectSettings/EditorSettings.asset') -notmatch '(?m)^\s*m_SerializationMode: 2\s*$') {
    $issues.Add('Force Text is required')
}
if ((Read-ProjectFile 'ProjectSettings/VersionControlSettings.asset') -notmatch '(?m)^\s*m_Mode: Visible Meta Files\s*$') {
    $issues.Add('Visible Meta Files is required')
}
$manifest = (Read-ProjectFile 'Packages/manifest.json' | ConvertFrom-Json).dependencies
$locked = (Read-ProjectFile 'Packages/packages-lock.json' | ConvertFrom-Json).dependencies
foreach ($entry in $manifest.PSObject.Properties) {
    $lockEntry = $locked.PSObject.Properties[$entry.Name]
    if ($null -eq $lockEntry -or $lockEntry.Value.version -ne $entry.Value -or $lockEntry.Value.depth -ne 0) {
        $issues.Add("Manifest/lock mismatch: $($entry.Name)")
    }
}

$assetsPath = Join-Path $root 'Assets'
if (-not (Test-Path -LiteralPath $assetsPath -PathType Container)) { throw 'Missing input: Assets' }
$assets = @(Get-ChildItem -LiteralPath $assetsPath -Recurse -Force | Where-Object {
    # Unity ignores dot-prefixed files/folders and temporary editor files.
    $_.FullName.Substring($assetsPath.Length) -notmatch '[/\\]\.' -and $_.Name -notmatch '~$'
})
$projectGuids = @{}
foreach ($asset in $assets) {
    if ($asset.Extension -ne '.meta') {
        if (-not (Test-Path -LiteralPath ($asset.FullName + '.meta'))) {
            $issues.Add("Missing meta: $($asset.FullName.Substring($root.Length + 1))")
        }
        continue
    }
    if (-not (Test-Path -LiteralPath $asset.FullName.Substring(0, $asset.FullName.Length - 5))) {
        $issues.Add("Orphan meta: $($asset.Name)")
    }
    $meta = [IO.File]::ReadAllText($asset.FullName)
    if ($meta -notmatch '(?m)^guid: ([a-fA-F0-9]{32})\s*$') {
        $issues.Add("Invalid meta GUID: $($asset.Name)")
    } else {
        $guid = $Matches[1].ToLowerInvariant()
        if ($projectGuids.ContainsKey($guid)) { $issues.Add("Duplicate GUID: $guid") }
        $projectGuids[$guid] = $asset.FullName
    }
}

$knownGuids = @{}
foreach ($guid in $projectGuids.Keys) { $knownGuids[$guid] = $true }
# Packages can be embedded or already imported; no package resolution or download here.
foreach ($packageRoot in @('Packages', 'Library/PackageCache')) {
    $path = Join-Path $root $packageRoot
    if (Test-Path -LiteralPath $path) {
        foreach ($meta in Get-ChildItem -LiteralPath $path -Filter '*.meta' -Recurse -File) {
            $content = [IO.File]::ReadAllText($meta.FullName)
            if ($content -match '(?m)^guid: ([a-fA-F0-9]{32})\s*$') {
                $knownGuids[$Matches[1].ToLowerInvariant()] = $true
            }
        }
    }
}
$referenceFiles = @($assets | Where-Object { -not $_.PSIsContainer -and $_.Extension -in @(
    '.unity', '.prefab', '.asset', '.mat', '.meta', '.scenetemplate', '.controller', '.anim') })
$referenceFiles += @(Get-ChildItem -LiteralPath (Join-Path $root 'ProjectSettings') -File)
foreach ($file in $referenceFiles) {
    $content = [IO.File]::ReadAllText($file.FullName)
    foreach ($match in [regex]::Matches($content, 'guid:\s*([a-fA-F0-9]{32})')) {
        $guid = $match.Groups[1].Value.ToLowerInvariant()
        # Unity's reserved built-in resource GUIDs.
        if ($guid -match '^0{16}[def]0{15}$' -or $guid -eq ('0' * 32)) { continue }
        if (-not $knownGuids.ContainsKey($guid)) {
            $issues.Add("Unresolved GUID $guid in $($file.FullName.Substring($root.Length + 1))")
        }
    }
}
foreach ($file in $assets | Where-Object { -not $_.PSIsContainer }) {
    $stream = [IO.File]::OpenRead($file.FullName)
    try {
        $buffer = New-Object byte[] 128
        $count = $stream.Read($buffer, 0, $buffer.Length)
        if ([Text.Encoding]::UTF8.GetString($buffer, 0, $count).StartsWith('version https://git-lfs.github.com/spec/v1')) {
            $issues.Add("Unmaterialized LFS pointer: $($file.Name)")
        }
    } finally { $stream.Dispose() }
}
if ($issues.Count -gt 0) { throw ($issues -join [Environment]::NewLine) }
Write-Output "PASS: static baseline; $($assets.Count) Assets entries; $($projectGuids.Count) project GUIDs. Live import NOT verified."
