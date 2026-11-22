# Изолированные проверки валидатора без Unity и сторонних тестовых пакетов.
$ErrorActionPreference = 'Stop'
$check = Join-Path $PSScriptRoot 'Check-Baseline.ps1'
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('ash-beacon-baseline-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
function Write-Fixture([string] $path, [string] $text) {
    $target = Join-Path $fixture $path
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
    [IO.File]::WriteAllText($target, $text)
}
function Expect-Failure([string] $name, [string] $expected) {
    $message = ''
    try { & $check -ProjectPath $fixture | Out-Null } catch { $message = $_.Exception.Message }
    if (-not $message.Contains($expected)) { throw "FAIL $name : expected '$expected', got '$message'" }
    Write-Output "PASS: $name"
}
Write-Fixture 'ProjectSettings/ProjectVersion.txt' 'm_EditorVersion: 6000.6.10f1'
Write-Fixture 'ProjectSettings/EditorSettings.asset' '  m_SerializationMode: 2'
Write-Fixture 'ProjectSettings/VersionControlSettings.asset' '  m_Mode: Visible Meta Files'
Write-Fixture 'Packages/manifest.json' '{"dependencies":{"com.example.test":"1.0.0"}}'
Write-Fixture 'Packages/packages-lock.json' '{"dependencies":{"com.example.test":{"version":"1.0.0","depth":0}}}'
$guid = '1234567890abcdef1234567890abcdef'
Write-Fixture 'Assets/Example.asset' "reference: {fileID: 1, guid: $guid}`nbuiltin: {guid: 0000000000000000e000000000000000}"
Write-Fixture 'Assets/Example.asset.meta' "guid: $guid"
& $check -ProjectPath $fixture
Write-Output 'PASS: valid fixture and builtin reference'
Write-Fixture 'ProjectSettings/ProjectVersion.txt' 'm_EditorVersion: 6000.2.0f1'
Expect-Failure 'wrong Editor' 'Editor version'
Write-Fixture 'ProjectSettings/ProjectVersion.txt' 'm_EditorVersion: 6000.6.10f1'
Write-Fixture 'ProjectSettings/EditorSettings.asset' 'm_SerializationMode: 0'
Expect-Failure 'binary serialization' 'Force Text'
Write-Fixture 'ProjectSettings/EditorSettings.asset' 'm_SerializationMode: 2'
Write-Fixture 'ProjectSettings/VersionControlSettings.asset' 'm_Mode: Hidden Meta Files'
Expect-Failure 'hidden meta' 'Visible Meta Files'
Write-Fixture 'ProjectSettings/VersionControlSettings.asset' 'm_Mode: Visible Meta Files'
Write-Fixture 'Packages/packages-lock.json' '{"dependencies":{"com.example.test":{"version":"2.0.0","depth":0}}}'
Expect-Failure 'package drift' 'Manifest/lock mismatch'
Write-Fixture 'Packages/packages-lock.json' '{"dependencies":{"com.example.test":{"version":"1.0.0","depth":0}}}'
Write-Fixture 'Assets/Second.asset' 'test'
Expect-Failure 'missing meta' 'Missing meta'
Write-Fixture 'Assets/Second.asset.meta' "guid: $guid"
Expect-Failure 'duplicate GUID' 'Duplicate GUID'
Write-Fixture 'Assets/Second.asset.meta' 'guid: abcdef1234567890abcdef1234567890'
Write-Fixture 'Assets/Second.asset' 'reference: {guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa}'
Expect-Failure 'broken reference' 'Unresolved GUID'
Write-Fixture 'Packages/Embedded/Target.asset' 'test'
Write-Fixture 'Packages/Embedded/Target.asset.meta' 'guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
& $check -ProjectPath $fixture
Write-Output 'PASS: embedded package reference'
Write-Fixture 'Assets/Second.asset' "version https://git-lfs.github.com/spec/v1`noid sha256:0000`nsize 100"
Expect-Failure 'LFS pointer' 'Unmaterialized LFS pointer'
Write-Fixture 'Assets/Second.asset' 'test'
Write-Fixture 'Assets/Orphan.meta' 'guid: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
Expect-Failure 'orphan meta' 'Orphan meta'
Write-Fixture 'Assets/Orphan' 'test'
Write-Fixture 'Assets/Orphan.meta' 'guid: invalid'
Expect-Failure 'invalid GUID' 'Invalid meta GUID'
Write-Fixture 'Assets/Orphan.meta' 'guid: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
& $check -ProjectPath $fixture
Write-Output 'PASS: restored fixture; 13 scenarios completed'
# Оставить небольшую fixture в Temp для разбора при необходимости; исходники проекта не меняются.
