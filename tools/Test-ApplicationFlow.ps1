# Независимая проверка чистого ядра. Не заменяет Unity Test Runner/PlayMode.
[CmdletBinding()]
param([string] $EditorData = 'C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Data')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$mono = Join-Path $EditorData 'MonoBleedingEdge/bin/mono.exe'
$compiler = Join-Path $EditorData 'MonoBleedingEdge/lib/mono/4.5/csc.exe'
$nunit = @(Get-ChildItem (Join-Path $root 'Library/PackageCache') -Filter nunit.framework.dll -Recurse)
if ($nunit.Count -ne 1) { throw 'Expected one restored NUnit framework; open the project to restore packages.' }
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('AshBeacon-CoreTests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$runner = Join-Path $scratch 'Runner.cs'
$executable = Join-Path $scratch 'CoreTests.exe'
@'
using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
class Runner
{
    static int Main()
    {
        var fixture = new AshBeacon.Tests.ApplicationFlowTests();
        int count = 0;
        foreach (var method in fixture.GetType().GetMethods())
        {
            if (!Attribute.IsDefined(method, typeof(TestAttribute))) continue;
            try
            {
                var task = method.Invoke(fixture, null) as Task;
                if (task != null) task.GetAwaiter().GetResult();
                Console.WriteLine("PASS " + method.Name);
                count++;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }
        Console.WriteLine("Pure C# tests: " + count + " PASS; Unity Test Runner NOT run.");
        return count == 6 ? 0 : 1;
    }
}
'@ | Set-Content -LiteralPath $runner -Encoding utf8
Copy-Item -LiteralPath $nunit[0].FullName -Destination (Join-Path $scratch 'nunit.framework.dll')
& $mono $compiler /nologo /target:exe "/out:$executable" "/reference:$($nunit[0].FullName)" `
    (Join-Path $root 'Assets/AshBeacon/Domain/GameSession.cs') `
    (Join-Path $root 'Assets/AshBeacon/Application/ApplicationFlow.cs') `
    (Join-Path $root 'Assets/AshBeacon/Tests/EditMode/ApplicationFlowTests.cs') $runner
if ($LASTEXITCODE -ne 0) { throw 'Core test compilation failed.' }
& $mono $executable
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
# Only explicit files created by this invocation; no recursive directory deletion.
Remove-Item -LiteralPath $runner,$executable,(Join-Path $scratch 'nunit.framework.dll')
Remove-Item -LiteralPath $scratch
