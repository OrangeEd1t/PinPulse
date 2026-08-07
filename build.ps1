$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root "src"
$out = Join-Path $root "CryptoMonitor.exe"
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (!(Test-Path $csc)) {
    throw "csc.exe was not found at $csc"
}

$sources = Get-ChildItem -Path $src -Filter "*.cs" | Sort-Object Name | ForEach-Object { $_.FullName }

& $csc /nologo /target:winexe /out:$out `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Web.Extensions.dll `
    /reference:System.Windows.Forms.dll `
    $sources

Write-Host "Built $out"
