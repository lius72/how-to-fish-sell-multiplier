param(
    [Parameter(Mandatory = $true)]
    [string]$GameDir,

    [Parameter(Mandatory = $true)]
    [string]$BepInExDir
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$Managed = Join-Path $GameDir "How to Fish_Data\Managed"
$Core = Join-Path $BepInExDir "BepInEx\core"
$Csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$Dist = Join-Path $Root "dist"

New-Item -ItemType Directory -Force -Path $Dist | Out-Null

$References = @(
    (Join-Path $Core "BepInEx.dll"),
    (Join-Path $Core "0Harmony.dll"),
    (Join-Path $Managed "Assembly-CSharp.dll"),
    (Join-Path $Managed "FishNet.Runtime.dll"),
    (Join-Path $Managed "UnityEngine.dll"),
    (Join-Path $Managed "UnityEngine.CoreModule.dll"),
    (Join-Path $Managed "netstandard.dll"),
    (Join-Path $Managed "System.Runtime.dll"),
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.dll",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Core.dll"
)

function Build-Plugin {
    param(
        [string]$Source,
        [string]$Output
    )

    $Arguments = @(
        "/nologo",
        "/target:library",
        "/platform:x64",
        "/optimize+",
        "/nowarn:0618",
        "/out:$Output"
    )

    foreach ($Reference in $References) {
        $Arguments += "/reference:$Reference"
    }

    $Arguments += $Source
    & $Csc @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Compilation failed for $Source"
    }
}

Build-Plugin `
    -Source (Join-Path $Root "src\SinglePlayer\Plugin.cs") `
    -Output (Join-Path $Dist "HowToFish.SellMultiplier.dll")

Build-Plugin `
    -Source (Join-Path $Root "src\Multiplayer\Plugin.cs") `
    -Output (Join-Path $Dist "HowToFish.SellMultiplier.Multiplayer.dll")

Write-Host "Build complete: $Dist"
