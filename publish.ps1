<#
.SYNOPSIS
	Publishes PauseMyBluetooth as single-file executables for x86, x64 and arm64.
.PARAMETER Configuration
	Build configuration (default: Release).
.PARAMETER SelfContained
	If set, the .NET runtime is bundled into the exe (larger, runs without installed .NET).
	Default: framework-dependent (requires installed .NET 8 Desktop Runtime).
.EXAMPLE
	.\publish.ps1
	.\publish.ps1 -SelfContained
#>
param(
	[string]$Configuration = "Release",
	[string[]]$Runtimes = @("win-x86", "win-x64", "win-arm64"),
	[switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$root       = $PSScriptRoot
$project    = Join-Path $root "PauseMyBluetooth.csproj"
$publishDir = Join-Path $root "Publish"
$SelfContained.IsPresent.ToString().ToLower()
$Compress = $SelfContained.IsPresent ? "true" : "false"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir | Out-Null

foreach ($rid in $Runtimes) {
	$arch   = $rid.Replace("win-", "")
	$outDir = Join-Path $publishDir $arch

	Write-Host "==> Publishing $rid ..." -ForegroundColor Cyan

	dotnet publish $project `
		-c $Configuration `
		-r $rid `
		-o $outDir `
		--self-contained $selfContained `
		-p:PublishSingleFile=true `
		-p:IncludeNativeLibrariesForSelfExtract=true `
		-p:IncludeAllContentForSelfExtract=true `
		-p:EnableCompressionInSingleFile=$selfContained `
		-p:DebugType=None `
		-p:DebugSymbols=false

	if ($LASTEXITCODE -ne 0) { throw "Publish failed for $rid" }

	# Keep only the exe, rename with architecture suffix
	$exe = Join-Path $outDir "PauseMyBluetooth.exe"
	$target = Join-Path $publishDir "PauseMyBluetooth-$arch.exe"
	Move-Item $exe $target -Force
	Remove-Item $outDir -Recurse -Force

	if($Compress){
		Compress-Archive -Path $target -DestinationPath (Join-Path $publishDir "PauseMyBluetooth-$arch.zip") -CompressionLevel Optimal -Force
	}
}

Write-Host "`nDone. Output:" -ForegroundColor Green
Get-ChildItem $publishDir -Include *.exe, *.zip -Recurse | ForEach-Object {
	"{0,-30} {1,8:N1} MB" -f $_.Name, ($_.Length / 1MB)
}
